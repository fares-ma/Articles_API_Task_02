namespace Core.Domain.Exceptions
{
    public class NewspaperNotFoundException : Exception
    {
        public NewspaperNotFoundException() : base("Newspaper not found.")
        {
        }

        public NewspaperNotFoundException(string message) : base(message)
        {
        }

        public NewspaperNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public NewspaperNotFoundException(int id) : base($"Newspaper with ID {id} not found.")
        {
        }

        public NewspaperNotFoundException(string name, bool byName = true) : base($"Newspaper with name '{name}' not found.")
        {
        }
    }
} 