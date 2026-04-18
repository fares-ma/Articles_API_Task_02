namespace Core.Domain.Exceptions
{
    public class S3ServiceException : Exception
    {
        public string Operation { get; }
        public string BucketName { get; }
        public string ObjectKey { get; }

        public S3ServiceException(string operation, string message) : base($"S3 {operation} failed: {message}")
        {
            Operation = operation;
        }

        public S3ServiceException(string operation, string bucketName, string objectKey, string message)
            : base($"S3 {operation} failed for bucket '{bucketName}', object '{objectKey}': {message}")
        {
            Operation = operation;
            BucketName = bucketName;
            ObjectKey = objectKey;
        }

        public S3ServiceException(string message) : base(message)
        {
        }

        public S3ServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 