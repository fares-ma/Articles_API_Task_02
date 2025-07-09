using Core.Domain.Models;
using Core.Services.Abstraction;
using Shared.Models;

namespace Core.Services
{
    public class NewspaperService : INewspaperService
    {
        private readonly INewspaperRepository _newspaperRepository;

        public NewspaperService(INewspaperRepository newspaperRepository)
        {
            _newspaperRepository = newspaperRepository;
        }

        public async Task<IEnumerable<Newspaper>> GetAllNewspapersAsync()
        {
            return await _newspaperRepository.GetAllAsync();
        }

        public async Task<PaginationResult<Newspaper>> GetAllNewspapersAsync(PaginationParameters parameters)
        {
            return await _newspaperRepository.GetAllAsync(parameters);
        }

        public async Task<Newspaper?> GetNewspaperByIdAsync(int id)
        {
            return await _newspaperRepository.GetByIdAsync(id);
        }

        public async Task<Newspaper?> GetNewspaperByNameAsync(string name)
        {
            return await _newspaperRepository.GetByNameAsync(name);
        }

        public async Task<IEnumerable<Newspaper>> GetActiveNewspapersAsync()
        {
            return await _newspaperRepository.GetActiveAsync();
        }

        public async Task<Newspaper> CreateNewspaperAsync(Newspaper newspaper)
        {
            newspaper.CreatedAt = DateTime.UtcNow;
            newspaper.IsActive = true;
            return await _newspaperRepository.AddAsync(newspaper);
        }

        public async Task<Newspaper> UpdateNewspaperAsync(Newspaper newspaper)
        {
            newspaper.UpdatedAt = DateTime.UtcNow;
            return await _newspaperRepository.UpdateAsync(newspaper);
        }

        public async Task<bool> DeleteNewspaperAsync(int id)
        {
            return await _newspaperRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _newspaperRepository.ExistsAsync(id);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _newspaperRepository.ExistsByNameAsync(name);
        }
    }
} 