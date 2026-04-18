namespace Core.Services.Abstraction
{
    public interface IS3FileProvider
    {
        // Basic CRUD operations
        Task UploadFileAsync(string filePath, string keyName, string? contentType = null);
        Task UploadStreamAsync(Stream stream, string keyName, string contentType);
        Task<string> GetFileContentAsStringAsync(string keyName);
        Task<string?> GetObjectByTitleAsync(string title);
        Task<List<string>> ListObjectsAsync(string? prefix = null, int? maxKeys = null);
        Task UpdateObjectAsync(string keyName, string content);
        Task DeleteObjectAsync(string keyName);
        Task<bool> ObjectExistsAsync(string keyName);
        
        // Enhanced operations
        Task<S3ObjectInfo?> GetObjectInfoAsync(string keyName);
        Task<List<S3ObjectInfo>> ListObjectsWithInfoAsync(string? prefix = null, int? maxKeys = null);
    }

    public class S3ObjectInfo
    {
        public string Key { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string ETag { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}