using Core.Domain.Models;
using Shared.Models;

namespace Core.Services.Abstraction
{
    public interface IArticleService
    {
        Task<Article?> GetArticleByIdAsync(int id);
        Task<Article?> GetArticleByTitleAsync(string title);
        Task<PaginationResult<Article>> GetAllArticlesAsync(PaginationParameters parameters);
        Task<PaginationResult<Article>> GetArticlesByTagAsync(string tag, PaginationParameters parameters);
        Task<PaginationResult<Article>> GetPublishedArticlesAsync(PaginationParameters parameters);
    }
} 