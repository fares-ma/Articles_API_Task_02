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
            var article = await _articleService.GetArticleByTitleAsync(title);
            
            if (article == null)
                return NotFound($"Article with title '{title}' not found");

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