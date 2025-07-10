using Core.Domain.Models;
using Shared.Models;

namespace Core.Services.Abstraction
{
    public interface INewspaperRepository : IRepository<Newspaper>
    {
        Task<PaginationResult<Newspaper>> GetAllAsync(PaginationParameters parameters);
        Task<Newspaper?> GetByNameAsync(string name);
        Task<IEnumerable<Newspaper>> GetActiveAsync();
        Task<bool> ExistsByNameAsync(string name);
    }
} 