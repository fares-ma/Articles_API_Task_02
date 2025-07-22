using AutoMapper;
using Core.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Models;
using Serilog;
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
        
        // Helper method to create pagination parameters with validation
        private PaginationParameters CreatePaginationParameters(int pageNumber, int pageSize)
        {
            // Ensure valid pagination values
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;
            
            return new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        
        // Helper method to map domain pagination result to DTO pagination result
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
        public async Task<ActionResult<ArticleDto>> GetArticleById(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            
            if (article == null)
                return NotFound($"Article with ID {id} not found");

            var articleDto = _mapper.Map<ArticleDto>(article);
            return Ok(articleDto);
        }

        // Dynamic endpoint that uses the flag to determine data source
        [HttpGet("title/{title}")]
        [Authorize]
        public async Task<ActionResult<ArticleDto>> GetArticleByTitle(string title)
        {
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
                return StatusCode(503, new { error = "Service is currently unavailable", details = ex.Message });
            }
        }

        [HttpGet("tag/{tag}")]
        public async Task<ActionResult<PaginationResult<ArticleDto>>> GetArticlesByTag(
            string tag,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var parameters = CreatePaginationParameters(pageNumber, pageSize);
            var result = await _articleService.GetArticlesByTagAsync(tag, parameters);
            return Ok(CreateDtoPaginationResult(result));
        }

        [HttpGet("published")]
        public async Task<ActionResult<PaginationResult<ArticleDto>>> GetPublishedArticles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var parameters = CreatePaginationParameters(pageNumber, pageSize);
            var result = await _articleService.GetPublishedArticlesAsync(parameters);
            return Ok(CreateDtoPaginationResult(result));
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
            var parameters = CreatePaginationParameters(pageNumber, pageSize);
            var result = await _articleService.GetArticlesByNewspaperAsync(newspaperId, parameters);
            return Ok(CreateDtoPaginationResult(result));
        }
    }
} 