using Microsoft.AspNetCore.Hosting;

namespace HRSystem.API.Services.Documents;

public class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveAsync(Stream content, int employeeId, string extension)
    {
        var ext = extension.StartsWith('.') ? extension.ToLowerInvariant() : "." + extension.ToLowerInvariant();
        var relativeDir = Path.Combine("uploads", "documents", employeeId.ToString());
        var fullDir = Path.Combine(_env.ContentRootPath, relativeDir);
        Directory.CreateDirectory(fullDir);

        var fileName = Guid.NewGuid().ToString("N") + ext;
        var relativePath = Path.Combine(relativeDir, fileName).Replace('\\', '/');
        var fullPath = Path.Combine(_env.ContentRootPath, relativePath);

        await using var stream = File.Create(fullPath);
        await content.CopyToAsync(stream);

        return relativePath;
    }

    public Task<Stream> OpenAsync(string filePath)
    {
        var fullPath = Path.Combine(_env.ContentRootPath, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Document file missing from disk", fullPath);
        Stream s = File.OpenRead(fullPath);
        return Task.FromResult(s);
    }

    public Task DeleteAsync(string filePath)
    {
        var fullPath = Path.Combine(_env.ContentRootPath, filePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
