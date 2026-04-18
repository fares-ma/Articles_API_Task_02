using AutoMapper;
using Core.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace Articles.Api.Controllers
{
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
            var parameters = CreatePaginationParameters(pageNumber, pageSize);
            var result = await _articleService.GetAllArticlesAsync(parameters);
            return Ok(CreateDtoPaginationResult(result));
        }
        
        private PaginationParameters CreatePaginationParameters(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;
            
            return new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        
        private PaginationResult<ArticleDto> CreateDtoPaginationResult<T>(PaginationResult<T> domainResult) 
            where T : Core.Domain.Models.Article
        {
            var dtos = _mapper.Map<IEnumerable<ArticleDto>>(domainResult.Items);
            
            return new PaginationResult<ArticleDto>
            {
                Items = dtos,
                TotalCount = domainResult.TotalCount,
                PageNumber = domainResult.PageNumber,
                PageSize = domainResult.PageSize,
                TotalPages = domainResult.TotalPages
            };
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ArticleDto>> GetArticleById(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            
            if (article == null)
                return NotFound($"Article with ID {id} not found");

            var articleDto = _mapper.Map<ArticleDto>(article);
            return Ok(articleDto);
        }

        [HttpGet("title/{title}")]
        [Authorize]
        public async Task<ActionResult<ArticleDto>> GetArticleByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
            {
                return BadRequest(new { error = "Title is required and must be 200 characters or fewer." });
            }

            _logger.LogInformation("Request received to get article by title: {Title}", title);

            try
            {
                var article = await _articleService.GetArticleByTitleAsync(title);

                if (article == null)
                {
                    _logger.LogWarning("No article found with title: {Title}", title);
                    return NotFound($"Article with title '{title}' not found");
                }

                _logger.LogInformation("Article found with title: {Title}", title);
                var articleDto = _mapper.Map<ArticleDto>(article);
                return Ok(articleDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Service error when retrieving article by title: {Title}", title);
                return StatusCode(503, new { error = "Service is currently unavailable" });
            }
        }

        [HttpGet("tag/{tag}")]
        [Authorize]
        public async Task<ActionResult<PaginationResult<ArticleDto>>> GetArticlesByTag(
            string tag,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(tag) || tag.Length > 100)
            {
                return BadRequest(new { error = "Tag is required and must be 100 characters or fewer." });
            }

            var parameters = CreatePaginationParameters(pageNumber, pageSize);
            var result = await _articleService.GetArticlesByTagAsync(tag, parameters);
            return Ok(CreateDtoPaginationResult(result));
        }

        [HttpGet("published")]
        [Authorize]
        public async Task<ActionResult<PaginationResult<ArticleDto>>> GetPublishedArticles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var parameters = CreatePaginationParameters(pageNumber, pageSize);
            var result = await _articleService.GetPublishedArticlesAsync(parameters);
            return Ok(CreateDtoPaginationResult(result));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ArticleDto>> CreateArticle(CreateArticleDto createArticleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var article = _mapper.Map<Core.Domain.Models.Article>(createArticleDto);
                var createdArticle = await _articleService.CreateArticleAsync(article);
                var articleDto = _mapper.Map<ArticleDto>(createdArticle);
                
                _logger.LogInformation("Article created successfully: {ArticleId} - {Title}", articleDto.Id, articleDto.Title);
                
                return CreatedAtAction(nameof(GetArticleById), new { id = articleDto.Id }, articleDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid article data: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Article creation failed: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating article");
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ArticleDto>> UpdateArticle(int id, UpdateArticleDto updateArticleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var article = _mapper.Map<Core.Domain.Models.Article>(updateArticleDto);
                article.Id = id;
                
                var updatedArticle = await _articleService.UpdateArticleAsync(article);
                var articleDto = _mapper.Map<ArticleDto>(updatedArticle);
                
                _logger.LogInformation("Article updated successfully: {ArticleId} - {Title}", articleDto.Id, articleDto.Title);
                
                return Ok(articleDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid article data for update: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Article update failed: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating article {ArticleId}", id);
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteArticle(int id)
        {
            try
            {
                var result = await _articleService.DeleteArticleAsync(id);
                if (result)
                {
                    _logger.LogInformation("Article deleted successfully: {ArticleId}", id);
                    return NoContent();
                }
                return NotFound(new { error = $"Article with ID {id} not found" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid article ID for deletion: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Article deletion failed: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting article {ArticleId}", id);
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        [HttpGet("newspaper/{newspaperId}")]
        [Authorize]
        public async Task<ActionResult<PaginationResult<ArticleDto>>> GetArticlesByNewspaper(
            int newspaperId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var parameters = CreatePaginationParameters(pageNumber, pageSize);
            var result = await _articleService.GetArticlesByNewspaperAsync(newspaperId, parameters);
            return Ok(CreateDtoPaginationResult(result));
        }


    }
}