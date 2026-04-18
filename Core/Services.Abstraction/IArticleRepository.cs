using Core.Domain.Models;
using Shared.Models;

namespace Core.Services.Abstraction
{
    public interface IArticleRepository : IRepository<Article>
    {
        Task<Article?> GetByTitleAsync(string title);
        Task<PaginationResult<Article>> GetByTagAsync(string tag, PaginationParameters parameters);
        Task<PaginationResult<Article>> GetPublishedAsync(PaginationParameters parameters);
        Task<PaginationResult<Article>> GetByNewspaperAsync(int newspaperId, PaginationParameters parameters);
        
        new Task<int> GetTotalCountAsync();
        Task<int> GetPublishedCountAsync();
        Task<int> GetByTagCountAsync(string tag);
        Task<bool> ExistsByTitleAsync(string title);
        Task<int> GetByNewspaperCountAsync(int newspaperId);
    }
} 