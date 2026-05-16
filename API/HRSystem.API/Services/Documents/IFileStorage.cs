namespace HRSystem.API.Services.Documents;

public interface IFileStorage
{
    // Saves the stream to a unique path under uploads/documents/{employeeId}/{Guid}.{ext}
    // and returns the relative FilePath (which is what the DB stores).
    Task<string> SaveAsync(Stream content, int employeeId, string extension);

    // Opens the file for streaming back to the client. Throws FileNotFoundException if missing.
    Task<Stream> OpenAsync(string filePath);

    // Deletes the file. No-op if already gone.
    Task DeleteAsync(string filePath);
}
