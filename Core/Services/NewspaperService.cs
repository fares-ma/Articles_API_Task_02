using Core.Domain.Models;
using Core.Services.Abstraction;
using Shared.Models;
using Core.Domain.Exceptions;

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
            if (await _newspaperRepository.ExistsByNameAsync(newspaper.Name))
            {
                throw new DuplicateEntityException("Newspaper", "name", newspaper.Name);
            }

            newspaper.CreatedAt = DateTime.UtcNow;
            newspaper.IsActive = true;
            return await _newspaperRepository.AddAsync(newspaper);
        }

        public async Task<Newspaper> UpdateNewspaperAsync(Newspaper newspaper)
        {
            var existingNewspaper = await _newspaperRepository.GetByIdAsync(newspaper.Id);
            if (existingNewspaper == null)
            {
                throw new NewspaperNotFoundException(newspaper.Id);
            }

            // Check for name duplication if changed
            if (newspaper.Name != existingNewspaper.Name && 
                await _newspaperRepository.ExistsByNameAsync(newspaper.Name))
            {
                throw new DuplicateEntityException("Newspaper", "name", newspaper.Name);
            }

            newspaper.UpdatedAt = DateTime.UtcNow;
            newspaper.CreatedAt = existingNewspaper.CreatedAt;
            newspaper.IsActive = existingNewspaper.IsActive;

            return await _newspaperRepository.UpdateAsync(newspaper);
        }

        public async Task<bool> DeleteNewspaperAsync(int id)
        {
            if (!await _newspaperRepository.ExistsAsync(id))
            {
                throw new NewspaperNotFoundException(id);
            }

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