using Core.Domain.Models;
using Core.Services.Abstraction;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly ApplicationDbContext _context;

        public ArticleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Article?> GetByIdAsync(int id)
        {
            return await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Article?> GetByTitleAsync(string title)
        {
            return await _context.Articles
                .FirstOrDefaultAsync(a => a.Title == title);
        }

        public async Task<IEnumerable<Article>> GetAllAsync()
        {
            return await _context.Articles
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetByTagAsync(string tag)
        {
            return await _context.Articles
                .Where(a => a.Tags.Contains(tag))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetPublishedAsync()
        {
            return await _context.Articles
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Articles.CountAsync();
        }

        public async Task<int> GetPublishedCountAsync()
        {
            return await _context.Articles
                .Where(a => a.IsPublished)
                .CountAsync();
        }

        public async Task<int> GetByTagCountAsync(string tag)
        {
            return await _context.Articles
                .Where(a => a.Tags.Contains(tag))
                .CountAsync();
        }

        public async Task<Article> AddAsync(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Article> UpdateAsync(Article article)
        {
            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return false;

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Articles.AnyAsync(a => a.Id == id);
        }

        public async Task<bool> ExistsByTitleAsync(string title)
        {
            return await _context.Articles.AnyAsync(a => a.Title == title);
        }

        public async Task<IEnumerable<Article>> GetByNewspaperAsync(int newspaperId)
        {
            return await _context.Articles
                .Where(a => a.NewspaperId == newspaperId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetByNewspaperCountAsync(int newspaperId)
        {
            return await _context.Articles
                .Where(a => a.NewspaperId == newspaperId)
                .CountAsync();
        }
    }
} 