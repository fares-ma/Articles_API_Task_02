using AutoMapper;
using Core.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Models;

namespace Articles.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly IMapper _mapper;

        public ArticlesController(IArticleService articleService, IMapper mapper)
        {
            _articleService = articleService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all articles with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 50)</param>
        /// <returns>Paginated list of articles</returns>
        [HttpGet]
        public async Task<ActionResult<PaginationResult<ArticleDto>>> GetAllArticles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _articleService.GetAllArticlesAsync(parameters);
            var articleDtos = _mapper.Map<IEnumerable<ArticleDto>>(result.Items);

            return Ok(new PaginationResult<ArticleDto>
            {
                Items = articleDtos,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            });
        }

        /// <summary>
        /// Get article by ID
        /// </summary>
        /// <param name="id">Article ID</param>
        /// <returns>Article details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleDto>> GetArticleById(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            
            if (article == null)
                return NotFound($"Article with ID {id} not found");

            var articleDto = _mapper.Map<ArticleDto>(article);
            return Ok(articleDto);
        }

        /// <summary>
        /// Get article by title
        /// </summary>
        /// <param name="title">Article title</param>
        /// <returns>Article details</returns>
        [HttpGet("title/{title}")]
        public async Task<ActionResult<ArticleDto>> GetArticleByTitle(string title)
        {
            var article = await _articleService.GetArticleByTitleAsync(title);
            
            if (article == null)
                return NotFound($"Article with title '{title}' not found");

            var articleDto = _mapper.Map<ArticleDto>(article);
            return Ok(articleDto);
        }

        /// <summary>
        /// Get articles by tag with pagination
        /// </summary>
        /// <param name="tag">Tag to filter by</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 50)</param>
        /// <returns>Paginated list of articles with the specified tag</returns>
        [HttpGet("tag/{tag}")]
        public async Task<ActionResult<PaginationResult<ArticleDto>>> GetArticlesByTag(
            string tag,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _articleService.GetArticlesByTagAsync(tag, parameters);
            var articleDtos = _mapper.Map<IEnumerable<ArticleDto>>(result.Items);

            return Ok(new PaginationResult<ArticleDto>
            {
                Items = articleDtos,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            });
        }

        /// <summary>
        /// Get published articles with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 50)</param>
        /// <returns>Paginated list of published articles</returns>
        [HttpGet("published")]
        public async Task<ActionResult<PaginationResult<ArticleDto>>> GetPublishedArticles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _articleService.GetPublishedArticlesAsync(parameters);
            var articleDtos = _mapper.Map<IEnumerable<ArticleDto>>(result.Items);

            return Ok(new PaginationResult<ArticleDto>
            {
                Items = articleDtos,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            });
        }
    }
} 