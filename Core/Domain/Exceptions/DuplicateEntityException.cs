namespace Core.Domain.Exceptions
{
    public class DuplicateEntityException : Exception
    {
        public string EntityName { get; }
        public string PropertyName { get; }
        public object PropertyValue { get; }

        public DuplicateEntityException(string entityName, string propertyName, object propertyValue)
            : base($"{entityName} with {propertyName} '{propertyValue}' already exists.")
        {
            EntityName = entityName;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }

        public DuplicateEntityException(string message) : base(message)
        {
        }

        public DuplicateEntityException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 