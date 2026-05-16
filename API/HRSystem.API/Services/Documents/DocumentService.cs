using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Data;
using HRSystem.API.DTOs.Documents;
using HRSystem.API.Models.Documents;

namespace HRSystem.API.Services.Documents;

public class DocumentService : IDocumentService
{
    private const long MaxFileSizeBytes = 10L * 1024L * 1024L;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg",
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/png",
        "image/jpeg",
    };

    private readonly AppDbContext _context;
    private readonly IFileStorage _storage;

    public DocumentService(AppDbContext context, IFileStorage storage)
    {
        _context = context;
        _storage = storage;
    }

    // ============== Documents ==============

    public async Task<EmployeeDocumentDto> UploadAsync(
        int employeeId,
        IFormFile file,
        int categoryId,
        DateTime? expiryDate,
        string? notes,
        int uploadedByEmployeeId)
    {
        if (file.Length == 0)
            throw new InvalidOperationException("File is empty");
        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"File exceeds 10 MB limit ({file.Length} bytes)");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"Disallowed file extension: {ext}");
        if (!AllowedContentTypes.Contains(file.ContentType ?? ""))
            throw new InvalidOperationException($"Disallowed content type: {file.ContentType}");

        var employeeExists = await _context.Employees.AnyAsync(e => e.Id == employeeId);
        if (!employeeExists)
            throw new InvalidOperationException($"Employee {employeeId} not found");

        var category = await _context.DocumentCategories.FindAsync(categoryId);
        if (category == null)
            throw new InvalidOperationException($"Category {categoryId} not found");

        await using var stream = file.OpenReadStream();
        var filePath = await _storage.SaveAsync(stream, employeeId, ext);

        var doc = new EmployeeDocument
        {
            EmployeeId = employeeId,
            CategoryId = categoryId,
            FileName = Path.GetFileName(file.FileName),
            FilePath = filePath,
            FileSize = file.Length,
            ContentType = file.ContentType ?? "application/octet-stream",
            ExpiryDate = expiryDate.HasValue
                ? DateTime.SpecifyKind(expiryDate.Value.Date, DateTimeKind.Utc)
                : null,
            UploadedById = uploadedByEmployeeId,
            Notes = notes,
        };
        _context.EmployeeDocuments.Add(doc);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Disk write succeeded but DB row failed — clean up the orphan file so it
            // doesn't accumulate untracked storage.
            try { await _storage.DeleteAsync(filePath); } catch { /* swallow cleanup error */ }
            throw;
        }

        return await MapAsync(doc);
    }

    public async Task<List<EmployeeDocumentDto>> ListByEmployeeAsync(int employeeId)
    {
        var rows = await _context.EmployeeDocuments
            .Where(d => d.EmployeeId == employeeId)
            .Include(d => d.Category)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
        return await MapListAsync(rows);
    }

    public async Task<(Stream Stream, string FileName, string ContentType)?> DownloadAsync(
        int documentId,
        int currentEmployeeId,
        bool isHrActor)
    {
        var doc = await _context.EmployeeDocuments.FindAsync(documentId);
        if (doc == null) return null;

        if (!isHrActor && doc.EmployeeId != currentEmployeeId)
            throw new UnauthorizedAccessException("Not authorized to download this document");

        var stream = await _storage.OpenAsync(doc.FilePath);
        return (stream, doc.FileName, doc.ContentType);
    }

    public async Task<bool> DeleteAsync(int documentId)
    {
        var doc = await _context.EmployeeDocuments.FindAsync(documentId);
        if (doc == null) return false;

        var filePath = doc.FilePath;
        _context.EmployeeDocuments.Remove(doc);
        await _context.SaveChangesAsync();

        // Disk delete runs after the DB commit. If it fails the file is an orphan that
        // ops can sweep later — far less harmful than a row pointing at a missing file.
        try { await _storage.DeleteAsync(filePath); } catch { /* swallow cleanup error */ }
        return true;
    }

    public async Task<List<EmployeeDocumentDto>> GetExpiringAsync(int daysAhead)
    {
        var today = DateTime.UtcNow.Date;
        var cutoff = today.AddDays(daysAhead);

        var rows = await _context.EmployeeDocuments
            .Where(d => d.ExpiryDate != null
                     && d.ExpiryDate.Value.Date >= today
                     && d.ExpiryDate.Value.Date <= cutoff)
            .Include(d => d.Category)
            .OrderBy(d => d.ExpiryDate)
            .ToListAsync();
        return await MapListAsync(rows);
    }

    // ============== Categories ==============

    public async Task<List<DocumentCategoryDto>> ListCategoriesAsync()
    {
        return await _context.DocumentCategories
            .OrderBy(c => c.Name)
            .Select(c => MapCategory(c))
            .ToListAsync();
    }

    public async Task<DocumentCategoryDto> CreateCategoryAsync(CreateDocumentCategoryDto dto)
    {
        var c = new DocumentCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            Slug = await ToUniqueSlugAsync(dto.Name, excludeId: null),
        };
        _context.DocumentCategories.Add(c);
        await _context.SaveChangesAsync();
        return MapCategory(c);
    }

    public async Task<DocumentCategoryDto?> UpdateCategoryAsync(int id, UpdateDocumentCategoryDto dto)
    {
        var c = await _context.DocumentCategories.FindAsync(id);
        if (c == null) return null;
        c.Name = dto.Name;
        c.Description = dto.Description;
        c.Slug = await ToUniqueSlugAsync(dto.Name, excludeId: id);
        await _context.SaveChangesAsync();
        return MapCategory(c);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var c = await _context.DocumentCategories.FindAsync(id);
        if (c == null) return false;
        var inUse = await _context.EmployeeDocuments.AnyAsync(d => d.CategoryId == id);
        if (inUse)
            throw new InvalidOperationException("Category is in use by one or more documents");
        _context.DocumentCategories.Remove(c);
        await _context.SaveChangesAsync();
        return true;
    }

    // ============== Helpers ==============

    private async Task<EmployeeDocumentDto> MapAsync(EmployeeDocument d)
    {
        var employeeName = await _context.Employees
            .Where(e => e.Id == d.EmployeeId)
            .Select(e => e.FirstName + " " + e.LastName)
            .FirstOrDefaultAsync();
        var categoryName = d.Category?.Name ?? await _context.DocumentCategories
            .Where(c => c.Id == d.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync() ?? "";
        return MapCore(d, employeeName, categoryName);
    }

    private async Task<List<EmployeeDocumentDto>> MapListAsync(List<EmployeeDocument> rows)
    {
        if (rows.Count == 0) return new List<EmployeeDocumentDto>();

        var employeeIds = rows.Select(r => r.EmployeeId).Distinct().ToList();
        var names = await _context.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.FirstName + " " + e.LastName);

        var missingCategoryIds = rows
            .Where(r => r.Category == null)
            .Select(r => r.CategoryId)
            .Distinct()
            .ToList();
        var fallbackCategoryNames = missingCategoryIds.Count == 0
            ? new Dictionary<int, string>()
            : await _context.DocumentCategories
                .Where(c => missingCategoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

        return rows.Select(d =>
        {
            var catName = d.Category?.Name ?? fallbackCategoryNames.GetValueOrDefault(d.CategoryId, "");
            return MapCore(d, names.GetValueOrDefault(d.EmployeeId), catName);
        }).ToList();
    }

    private static EmployeeDocumentDto MapCore(EmployeeDocument d, string? employeeName, string categoryName) => new()
    {
        Id = d.Id,
        PublicId = d.PublicId,
        EmployeeId = d.EmployeeId,
        EmployeeName = employeeName,
        CategoryId = d.CategoryId,
        CategoryName = categoryName,
        FileName = d.FileName,
        FileSize = d.FileSize,
        ContentType = d.ContentType,
        ExpiryDate = d.ExpiryDate,
        UploadedById = d.UploadedById,
        Notes = d.Notes,
        CreatedAt = d.CreatedAt,
    };

    private static DocumentCategoryDto MapCategory(DocumentCategory c) => new()
    {
        Id = c.Id,
        PublicId = c.PublicId,
        Name = c.Name,
        Description = c.Description,
        Slug = c.Slug,
        CreatedAt = c.CreatedAt,
    };

    private static string Slugify(string s)
    {
        var lower = s.ToLowerInvariant();
        var sb = new System.Text.StringBuilder();
        foreach (var c in lower)
        {
            if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }
        return sb.ToString().Trim('-');
    }

    private async Task<string> ToUniqueSlugAsync(string name, int? excludeId)
    {
        var baseSlug = Slugify(name);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "category";
        var candidate = baseSlug;
        var suffix = 1;
        while (await _context.DocumentCategories
            .AnyAsync(c => c.Slug == candidate && (excludeId == null || c.Id != excludeId)))
        {
            suffix++;
            candidate = $"{baseSlug}-{suffix}";
        }
        return candidate;
    }
}
