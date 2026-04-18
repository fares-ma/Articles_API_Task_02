using System.ComponentModel.DataAnnotations;

namespace Core.Domain.Exceptions
{
    public class ValidationException : Exception
    {
        public List<ValidationResult> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<ValidationResult>();
        }

        public ValidationException(string message, List<ValidationResult> errors) : base(message)
        {
            Errors = errors ?? new List<ValidationResult>();
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
            Errors = new List<ValidationResult>();
        }

        public ValidationException(List<ValidationResult> errors) : base("Validation failed.")
        {
            Errors = errors ?? new List<ValidationResult>();
        }
    }
} 