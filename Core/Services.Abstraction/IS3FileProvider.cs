using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Services.Abstraction
{
    public interface IS3FileProvider
    {
        Task UploadFileAsync(string filePath, string keyName);
        Task DownloadFileAsync(string keyName, string destinationPath);
        Task<List<string>> ListObjectsAsync(string prefix = null);
        Task DeleteObjectAsync(string keyName);
    }
} 