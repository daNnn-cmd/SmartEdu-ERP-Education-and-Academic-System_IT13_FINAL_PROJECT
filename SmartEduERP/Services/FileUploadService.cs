
using System.Linq;

namespace SmartEduERP.Services;

public class FileUploadService
{
    private readonly string _uploadRootPath;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
    private readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };

    public FileUploadService()
    {
        // Use MAUI's AppDataDirectory for file storage
        _uploadRootPath = Path.Combine(FileSystem.AppDataDirectory, "uploads");
        EnsureUploadDirectories();
    }

    private void EnsureUploadDirectories()
    {
        var directories = new[] { "students", "teachers", "documents", "receipts" };

        foreach (var dir in directories)
        {
            var path = Path.Combine(_uploadRootPath, dir);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    
    public bool DeleteFile(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool FileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        return File.Exists(filePath);
    }

    public string GetFileUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return "/img/default-avatar.png";

        return filePath;
    }
}

public class UploadResult
{
    public bool Success { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
