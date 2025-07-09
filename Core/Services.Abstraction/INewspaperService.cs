using Core.Domain.Models;
using Shared.Models;

namespace Core.Services.Abstraction
{
    public interface INewspaperService
    {
        Task<IEnumerable<Newspaper>> GetAllNewspapersAsync();
        Task<PaginationResult<Newspaper>> GetAllNewspapersAsync(PaginationParameters parameters);
        Task<Newspaper?> GetNewspaperByIdAsync(int id);
        Task<Newspaper?> GetNewspaperByNameAsync(string name);
        Task<IEnumerable<Newspaper>> GetActiveNewspapersAsync();
        Task<Newspaper> CreateNewspaperAsync(Newspaper newspaper);
        Task<Newspaper> UpdateNewspaperAsync(Newspaper newspaper);
        Task<bool> DeleteNewspaperAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
    }
} 