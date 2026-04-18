using Core.Domain.Models;
using Core.Services.Abstraction;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Infrastructure.Persistence.Repositories
{

    #region summary
    /// ArticleRepository implements data access operations for Article entities.
    /// 
    /// Purpose:
    /// - Provides data access layer for article operations
    /// - Implements article-specific query methods
    /// - Handles database operations with Entity Framework
    /// - Provides optimized queries for filtering and searching
    /// - Manages article-newspaper relationships
    /// 
    /// Dependencies:
    /// - ApplicationDbContext for database operations
    /// - Entity Framework Core for ORM functionality
    /// - Core.Domain.Models for entity definitions
    /// - IArticleRepository interface for contract
    /// 
    /// Alternatives:
    /// - Could implement Dapper for raw SQL performance
    /// - Could add query caching for frequently accessed data
    /// - Could implement read/write separation
    /// - Could add database sharding for large datasets 
    #endregion

    public class ArticleRepository : Repository<Article>, IArticleRepository
    {
        private readonly new ApplicationDbContext _context;

        public ArticleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Article?> GetByTitleAsync(string title)
        {
            return await _context.Articles
                .FirstOrDefaultAsync(a => a.Title == title);
        }

        public async Task<PaginationResult<Article>> GetByTagAsync(string tag, PaginationParameters parameters)
        {
            if (parameters.PageNumber < 1) parameters.PageNumber = 1;
            if (parameters.PageSize < 1) parameters.PageSize = 10;
            if (parameters.PageSize > 50) parameters.PageSize = 50;

            var totalCount = await _context.Articles
                .Where(a => a.Tags.Contains(tag))
                .CountAsync();

            var items = await _context.Articles
                .Where(a => a.Tags.Contains(tag))
                .OrderByDescending(a => a.CreatedAt)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PaginationResult<Article>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
            };
        }

        public async Task<PaginationResult<Article>> GetPublishedAsync(PaginationParameters parameters)
        {
            if (parameters.PageNumber < 1) parameters.PageNumber = 1;
            if (parameters.PageSize < 1) parameters.PageSize = 10;
            if (parameters.PageSize > 50) parameters.PageSize = 50;

            var totalCount = await _context.Articles
                .Where(a => a.IsPublished)
                .CountAsync();

            var items = await _context.Articles
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PaginationResult<Article>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
            };
        }

        public new async Task<int> GetTotalCountAsync()
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

        public async Task<bool> ExistsByTitleAsync(string title)
        {
            return await _context.Articles.AnyAsync(a => a.Title == title);
        }

        public async Task<PaginationResult<Article>> GetByNewspaperAsync(int newspaperId, PaginationParameters parameters)
        {
            if (parameters.PageNumber < 1) parameters.PageNumber = 1;
            if (parameters.PageSize < 1) parameters.PageSize = 10;
            if (parameters.PageSize > 50) parameters.PageSize = 50;

            var totalCount = await _context.Articles
                .Where(a => a.NewspaperId == newspaperId)
                .CountAsync();

            var items = await _context.Articles
                .Where(a => a.NewspaperId == newspaperId)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PaginationResult<Article>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
            };
        }

        public async Task<int> GetByNewspaperCountAsync(int newspaperId)
        {
            return await _context.Articles
                .Where(a => a.NewspaperId == newspaperId)
                .CountAsync();
        }
    }
} 