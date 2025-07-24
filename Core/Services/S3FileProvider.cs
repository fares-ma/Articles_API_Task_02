using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Core.Services.Abstraction;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public class S3FileProvider : IS3FileProvider
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3FileProvider(IAmazonS3 s3Client, IConfiguration config)
        {
            _s3Client = s3Client;
            _bucketName = config["AWS:BucketName"];
        }

        public async Task UploadFileAsync(string filePath, string keyName)
        {
            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(filePath, _bucketName, keyName);
        }

        public async Task DownloadFileAsync(string keyName, string destinationPath)
        {
            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.DownloadAsync(destinationPath, _bucketName, keyName);
        }

        public async Task<List<string>> ListObjectsAsync(string prefix = null)
        {
            var request = new ListObjectsV2Request { BucketName = _bucketName, Prefix = prefix };
            var response = await _s3Client.ListObjectsV2Async(request);
            return response.S3Objects.Select(o => o.Key).ToList();
        }

        public async Task DeleteObjectAsync(string keyName)
        {
            await _s3Client.DeleteObjectAsync(new DeleteObjectRequest { BucketName = _bucketName, Key = keyName });
        }
    }
} 