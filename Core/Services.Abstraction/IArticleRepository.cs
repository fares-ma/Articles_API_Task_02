using Core.Domain.Models;
using Shared.Models;

namespace Core.Services.Abstraction
{
    public interface IArticleRepository : IRepository<Article>
    {
        Task<Article?> GetByTitleAsync(string title);
        Task<IEnumerable<Article>> GetByTagAsync(string tag);
        Task<IEnumerable<Article>> GetPublishedAsync();
        Task<int> GetTotalCountAsync();
        Task<int> GetPublishedCountAsync();
        Task<int> GetByTagCountAsync(string tag);
        Task<bool> ExistsByTitleAsync(string title);
        Task<IEnumerable<Article>> GetByNewspaperAsync(int newspaperId);
        Task<int> GetByNewspaperCountAsync(int newspaperId);
    }
} 