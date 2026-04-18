using Amazon.S3;
using Amazon.S3.Model;
using Core.Services.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Core.Domain.Exceptions;
using System.Text;
using System.Linq;

namespace Core.Services
{
    /// <summary>
    /// Simple S3 file provider with basic CRUD operations
    /// </summary>
    public class S3FileProvider : IS3FileProvider
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly ILogger<S3FileProvider> _logger;

        public S3FileProvider(
            IAmazonS3 s3Client, 
            IConfiguration configuration, 
            ILogger<S3FileProvider> logger)
        {
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _bucketName = configuration["AWS:BucketName"] 
                ?? throw new ArgumentNullException("AWS:BucketName configuration is missing");
        }

        // CREATE - Upload file
        public async Task UploadFileAsync(string filePath, string keyName, string? contentType = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
                }

                if (string.IsNullOrWhiteSpace(keyName))
                {
                    throw new ArgumentException("Key name cannot be null or empty", nameof(keyName));
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                _logger.LogInformation("Uploading file {FilePath} to S3 with key {KeyName}", filePath, keyName);

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName,
                    FilePath = filePath,
                    ContentType = contentType ?? "application/octet-stream"
                };

                await _s3Client.PutObjectAsync(request);
                _logger.LogInformation("Successfully uploaded file {KeyName} to S3", keyName);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "S3 error uploading file {KeyName}: {ErrorCode}", keyName, ex.ErrorCode);
                throw new S3ServiceException("upload", $"S3 error uploading file: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading file {KeyName}", keyName);
                throw new S3ServiceException("upload", $"Failed to upload file: {ex.Message}");
            }
        }

        // READ - Get object by key
        public async Task<string> GetFileContentAsStringAsync(string keyName)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var reader = new StreamReader(response.ResponseStream);
                return await reader.ReadToEndAsync();
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new S3ServiceException("get", $"Object '{keyName}' not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting object {KeyName}", keyName);
                throw new S3ServiceException("get", $"Failed to get object: {ex.Message}");
            }
        }

        // READ - Get object by title (searches for objects with title in metadata or key)
        public async Task<string?> GetObjectByTitleAsync(string title)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    throw new ArgumentException("Title cannot be null or empty", nameof(title));
                }

                _logger.LogInformation("Searching for object by title: {Title}", title);

                // First, try to find by exact key match
                var exactKey = $"articles/{title.ToLower().Replace(" ", "-")}.json";
                if (await ObjectExistsAsync(exactKey))
                {
                    _logger.LogInformation("Found exact match for title: {Title} at key: {Key}", title, exactKey);
                    return await GetFileContentAsStringAsync(exactKey);
                }

                // If not found, search through all objects
                var objects = await ListObjectsAsync("articles/");
                var matchingKey = objects.FirstOrDefault(key => 
                    key.Contains(title, StringComparison.OrdinalIgnoreCase) ||
                    Path.GetFileNameWithoutExtension(key).Contains(title.Replace(" ", "-"), StringComparison.OrdinalIgnoreCase));

                if (matchingKey != null)
                {
                    _logger.LogInformation("Found matching object for title: {Title} at key: {Key}", title, matchingKey);
                    return await GetFileContentAsStringAsync(matchingKey);
                }

                _logger.LogWarning("No object found with title: {Title}", title);
                return null;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting object by title {Title}", title);
                throw new S3ServiceException("get", $"Failed to get object by title: {ex.Message}");
            }
        }

        // READ - List all objects
        public async Task<List<string>> ListObjectsAsync(string? prefix = null, int? maxKeys = null)
        {
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix,
                    MaxKeys = maxKeys ?? 1000
                };

                var response = await _s3Client.ListObjectsV2Async(request);
                return response.S3Objects.Select(o => o.Key).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing objects");
                throw new S3ServiceException("list", $"Failed to list objects: {ex.Message}");
            }
        }

        // UPDATE - Update existing object
        public async Task UpdateObjectAsync(string keyName, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyName))
                {
                    throw new ArgumentException("Key name cannot be null or empty", nameof(keyName));
                }

                if (content == null)
                {
                    throw new ArgumentNullException(nameof(content));
                }

                _logger.LogInformation("Updating object {KeyName} in S3", keyName);

                var contentType = keyName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) 
                    ? "application/json" 
                    : "text/plain";

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName,
                    ContentBody = content,
                    ContentType = contentType
                };

                await _s3Client.PutObjectAsync(request);
                _logger.LogInformation("Successfully updated object {KeyName} in S3", keyName);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "S3 error updating object {KeyName}: {ErrorCode}", keyName, ex.ErrorCode);
                throw new S3ServiceException("update", $"S3 error updating object: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating object {KeyName}", keyName);
                throw new S3ServiceException("update", $"Failed to update object: {ex.Message}");
            }
        }

        // DELETE - Delete object
        public async Task DeleteObjectAsync(string keyName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyName))
                {
                    throw new ArgumentException("Key name cannot be null or empty", nameof(keyName));
                }

                _logger.LogInformation("Deleting object {KeyName} from S3", keyName);

                // Check if object exists before attempting to delete
                if (!await ObjectExistsAsync(keyName))
                {
                    _logger.LogWarning("Object {KeyName} does not exist in S3", keyName);
                    throw new S3ServiceException("delete", $"Object '{keyName}' not found");
                }

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName
                };

                await _s3Client.DeleteObjectAsync(request);
                _logger.LogInformation("Successfully deleted object {KeyName} from S3", keyName);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (S3ServiceException)
            {
                throw;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "S3 error deleting object {KeyName}: {ErrorCode}", keyName, ex.ErrorCode);
                throw new S3ServiceException("delete", $"S3 error deleting object: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting object {KeyName}", keyName);
                throw new S3ServiceException("delete", $"Failed to delete object: {ex.Message}");
            }
        }

        // Helper methods for interface compatibility
        public async Task UploadStreamAsync(Stream stream, string keyName, string contentType)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName,
                    InputStream = stream,
                    ContentType = contentType
                };

                await _s3Client.PutObjectAsync(request);
            }
            catch (Exception ex)
            {
                throw new S3ServiceException("upload", $"Failed to upload stream: {ex.Message}");
            }
        }

        public async Task<bool> ObjectExistsAsync(string keyName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyName))
                {
                    return false;
                }

                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = keyName
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if object exists: {KeyName}", keyName);
                return false;
            }
        }

        // Enhanced operations
        public async Task<S3ObjectInfo?> GetObjectInfoAsync(string keyName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyName))
                {
                    throw new ArgumentException("Key name cannot be null or empty", nameof(keyName));
                }

                _logger.LogInformation("Getting object info for {KeyName}", keyName);

                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = keyName
                };

                var response = await _s3Client.GetObjectMetadataAsync(request);

                return new S3ObjectInfo
                {
                    Key = keyName,
                    Size = response.ContentLength,
                    LastModified = response.LastModified ?? DateTime.UtcNow,
                    ETag = response.ETag ?? string.Empty,
                    ContentType = response.Headers.ContentType ?? "application/octet-stream",
                    Metadata = response.Metadata?.Keys.ToDictionary(key => key, key => response.Metadata[key]) ?? new Dictionary<string, string>()
                };
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Object not found: {KeyName}", keyName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting object info for {KeyName}", keyName);
                throw new S3ServiceException("getInfo", $"Failed to get object info: {ex.Message}");
            }
        }

        public async Task<List<S3ObjectInfo>> ListObjectsWithInfoAsync(string? prefix = null, int? maxKeys = null)
        {
            try
            {
                _logger.LogInformation("Listing objects with info - Prefix: {Prefix}, MaxKeys: {MaxKeys}", prefix ?? "none", maxKeys?.ToString() ?? "unlimited");

                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix,
                    MaxKeys = maxKeys ?? 1000
                };

                var response = await _s3Client.ListObjectsV2Async(request);
                var objectInfos = new List<S3ObjectInfo>();

                foreach (var s3Object in response.S3Objects)
                {
                    objectInfos.Add(new S3ObjectInfo
                    {
                        Key = s3Object.Key,
                        Size = s3Object.Size ?? 0,
                        LastModified = s3Object.LastModified ?? DateTime.UtcNow,
                        ETag = s3Object.ETag ?? string.Empty,
                        ContentType = "application/octet-stream", // Default, would need separate call for exact content type
                        Metadata = new Dictionary<string, string>() // Would need separate calls for metadata
                    });
                }

                _logger.LogInformation("Found {Count} objects with info", objectInfos.Count);
                return objectInfos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing objects with info");
                throw new S3ServiceException("listWithInfo", $"Failed to list objects with info: {ex.Message}");
            }
        }
    }
}