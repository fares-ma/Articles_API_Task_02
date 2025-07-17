using AutoMapper;
using Core.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Models;
using Serilog;

namespace Articles.Api.Controllers
{

    #region summary
    /// ArticlesController handles HTTP requests for article-related operations.
    /// 
    /// Purpose:
    /// - Provides RESTful API endpoints for article management
    /// - Handles CRUD operations for articles
    /// - Implements pagination and filtering capabilities
    /// - Manages article-newspaper relationships
    /// - Provides search functionality by title, tags, and publication status
    /// 
    /// Dependencies:
    /// - IArticleService for business logic operations
    /// - AutoMapper for DTO transformations
    /// - ASP.NET Core MVC framework
    /// - Shared DTOs and models for data transfer
    /// 
    /// Alternatives:
    /// - Could implement GraphQL for more flexible queries
    /// - Could add caching layer for performance
    /// - Could implement real-time updates with SignalR
    /// - Could add authentication and authorization 
    #endregion

    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly IMapper _mapper;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(IArticleService articleService, IMapper mapper, ILogger<ArticlesController> logger)
        {
            _articleService = articleService;
            _mapper = mapper;
            _logger = logger;
        }

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

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleDto>> GetArticleById(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            
            if (article == null)
                return NotFound($"Article with ID {id} not found");

            var articleDto = _mapper.Map<ArticleDto>(article);
            return Ok(articleDto);
        }

       
        [HttpGet("title/{title}")]
        public async Task<ActionResult<ArticleDto>> GetArticleByTitle(string title)
        {
            
            var safeTitle = string.Join("_", title.Split(System.IO.Path.GetInvalidFileNameChars()));
            var fileName = $"Logs/article_{safeTitle}_{DateTime.Now:yyyyMMddHHmmssfff}.txt";

            
            var requestLogger = new Serilog.LoggerConfiguration()
                .WriteTo.File(fileName)
                .CreateLogger();

            requestLogger.Information("Request received to get article by title: {Title}", title);

            var article = await _articleService.GetArticleByTitleAsync(title);

            if (article == null)
            {
                requestLogger.Warning("No article found with title: {Title}", title);
                return NotFound($"Article with title '{title}' not found");
            }

            requestLogger.Information("Article found with title: {Title}", title);
            var articleDto = _mapper.Map<ArticleDto>(article);
            return Ok(articleDto);
        }

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

        [HttpPost]
        public async Task<ActionResult<ArticleDto>> CreateArticle(CreateArticleDto createArticleDto)
        {
            try
            {
                var article = _mapper.Map<Core.Domain.Models.Article>(createArticleDto);
                var createdArticle = await _articleService.CreateArticleAsync(article);
                var articleDto = _mapper.Map<ArticleDto>(createdArticle);
                
                return CreatedAtAction(nameof(GetArticleById), new { id = articleDto.Id }, articleDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ArticleDto>> UpdateArticle(int id, UpdateArticleDto updateArticleDto)
        {
            try
            {
                var article = _mapper.Map<Core.Domain.Models.Article>(updateArticleDto);
                article.Id = id;
                
                var updatedArticle = await _articleService.UpdateArticleAsync(article);
                var articleDto = _mapper.Map<ArticleDto>(updatedArticle);
                
                return Ok(articleDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteArticle(int id)
        {
            try
            {
                var result = await _articleService.DeleteArticleAsync(id);
                if (result)
                {
                    return NoContent();
                }
                return NotFound($"Article with ID {id} not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("newspaper/{newspaperId}")]
        public async Task<ActionResult<PaginationResult<ArticleDto>>> GetArticlesByNewspaper(
            int newspaperId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _articleService.GetArticlesByNewspaperAsync(newspaperId, parameters);
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