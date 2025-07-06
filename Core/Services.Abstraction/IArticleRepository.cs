using Core.Domain.Models;
using Shared.Models;

namespace Core.Services.Abstraction
{
    public interface IArticleRepository
    {
        Task<Article?> GetByIdAsync(int id);
        Task<Article?> GetByTitleAsync(string title);
        Task<IEnumerable<Article>> GetAllAsync();
        Task<IEnumerable<Article>> GetByTagAsync(string tag);
        Task<IEnumerable<Article>> GetPublishedAsync();
        Task<int> GetTotalCountAsync();
        Task<int> GetPublishedCountAsync();
        Task<int> GetByTagCountAsync(string tag);
    }
} 