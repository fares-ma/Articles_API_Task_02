using Core.Domain.Models;
using Core.Services.Abstraction;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Infrastructure.Persistence.Repositories
{
    public class NewspaperRepository : Repository<Newspaper>, INewspaperRepository
    {
        private readonly ApplicationDbContext _context;

        public NewspaperRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginationResult<Newspaper>> GetAllAsync(PaginationParameters parameters)
        {
            var query = _context.Newspapers
                .Include(n => n.Articles)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var newspapers = await query
                .OrderBy(n => n.Name)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PaginationResult<Newspaper>
            {
                Items = newspapers,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
            };
        }

        public async Task<Newspaper?> GetByNameAsync(string name)
        {
            return await _context.Newspapers
                .Include(n => n.Articles)
                .FirstOrDefaultAsync(n => n.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Newspaper>> GetActiveAsync()
        {
            return await _context.Newspapers
                .Include(n => n.Articles)
                .Where(n => n.IsActive)
                .OrderBy(n => n.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Newspapers.AnyAsync(n => n.Name.ToLower() == name.ToLower());
        }
    }
} 