using Core.Domain.Models;
using Shared.Models;

namespace Core.Services.Abstraction
{
    public interface INewspaperRepository
    {
        Task<IEnumerable<Newspaper>> GetAllAsync();
        Task<PaginationResult<Newspaper>> GetAllAsync(PaginationParameters parameters);
        Task<Newspaper?> GetByIdAsync(int id);
        Task<Newspaper?> GetByNameAsync(string name);
        Task<IEnumerable<Newspaper>> GetActiveAsync();
        Task<Newspaper> AddAsync(Newspaper newspaper);
        Task<Newspaper> UpdateAsync(Newspaper newspaper);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
    }
} 