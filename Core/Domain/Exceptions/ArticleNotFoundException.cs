namespace Core.Domain.Exceptions
{
    public class ArticleNotFoundException : Exception
    {
        public ArticleNotFoundException() : base("Article not found.")
        {
        }

        public ArticleNotFoundException(string message) : base(message)
        {
        }

        public ArticleNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ArticleNotFoundException(int id) : base($"Article with ID {id} not found.")
        {
        }

        public ArticleNotFoundException(string title, bool byTitle = true) : base($"Article with title '{title}' not found.")
        {
        }
    }
} 