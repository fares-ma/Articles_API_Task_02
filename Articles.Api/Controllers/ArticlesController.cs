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
            catch (InvalidOperationException ex) when (ex.Message.Contains("S3 service is currently unavailable") || ex.Message.Contains("S3 service error") || ex.Message.Contains("Error reading from S3"))
            {
                _logger.LogError(ex, "S3 error while getting article by title");
                return StatusCode(503, "S3 server is currently unavailable. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting article by title");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
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

        [HttpPost("s3/upload")]
        public async Task<IActionResult> UploadFileToS3([FromQuery] string filePath, [FromQuery] string keyName)
        {
            try
            {
                await _articleService.UploadFileToS3Async(filePath, keyName);
                return Ok("Upload completed");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("S3 service is currently unavailable") || ex.Message.Contains("S3 service error") || ex.Message.Contains("Error reading from S3"))
            {
                _logger.LogError(ex, "S3 error while uploading file");
                return StatusCode(503, "S3 server is currently unavailable. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while uploading file to S3");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet("s3/download")]
        public async Task<IActionResult> DownloadFileFromS3([FromQuery] string keyName, [FromQuery] string destinationPath)
        {
            try
            {
                await _articleService.DownloadFileFromS3Async(keyName, destinationPath);
                return Ok("Download completed");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("S3 service is currently unavailable") || ex.Message.Contains("S3 service error") || ex.Message.Contains("Error reading from S3"))
            {
                _logger.LogError(ex, "S3 error while downloading file");
                return StatusCode(503, "S3 server is currently unavailable. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while downloading file from S3");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet("s3/list")]
        public async Task<IActionResult> ListS3Objects([FromQuery] string prefix = null)
        {
            try
            {
                var result = await _articleService.ListS3ObjectsAsync(prefix);
                return Ok(result);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("S3 service is currently unavailable") || ex.Message.Contains("S3 service error") || ex.Message.Contains("Error reading from S3"))
            {
                _logger.LogError(ex, "S3 error while listing objects");
                return StatusCode(503, "S3 server is currently unavailable. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while listing S3 objects");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpDelete("s3/delete")]
        public async Task<IActionResult> DeleteS3Object([FromQuery] string keyName)
        {
            try
            {
                await _articleService.DeleteS3ObjectAsync(keyName);
                return Ok("Object deleted successfully");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("S3 service is currently unavailable") || ex.Message.Contains("S3 service error") || ex.Message.Contains("Error reading from S3"))
            {
                _logger.LogError(ex, "S3 error while deleting object");
                return StatusCode(503, "S3 server is currently unavailable. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting S3 object");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
    }
} 