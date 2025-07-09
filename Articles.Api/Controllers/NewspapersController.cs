using AutoMapper;
using Core.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Models;

namespace Articles.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewspapersController : ControllerBase
    {
        private readonly INewspaperService _newspaperService;
        private readonly IMapper _mapper;

        public NewspapersController(INewspaperService newspaperService, IMapper mapper)
        {
            _newspaperService = newspaperService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResult<NewspaperDto>>> GetAllNewspapers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _newspaperService.GetAllNewspapersAsync(parameters);
            var newspaperDtos = _mapper.Map<IEnumerable<NewspaperDto>>(result.Items);

            return Ok(new PaginationResult<NewspaperDto>
            {
                Items = newspaperDtos,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            });
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<NewspaperDto>>> GetAllNewspapersWithoutPagination()
        {
            var newspapers = await _newspaperService.GetAllNewspapersAsync();
            var newspaperDtos = _mapper.Map<IEnumerable<NewspaperDto>>(newspapers);
            return Ok(newspaperDtos);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<NewspaperDto>>> GetActiveNewspapers()
        {
            var newspapers = await _newspaperService.GetActiveNewspapersAsync();
            var newspaperDtos = _mapper.Map<IEnumerable<NewspaperDto>>(newspapers);
            return Ok(newspaperDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NewspaperDto>> GetNewspaperById(int id)
        {
            var newspaper = await _newspaperService.GetNewspaperByIdAsync(id);
            
            if (newspaper == null)
                return NotFound($"Newspaper with ID {id} not found");

            var newspaperDto = _mapper.Map<NewspaperDto>(newspaper);
            return Ok(newspaperDto);
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<NewspaperDto>> GetNewspaperByName(string name)
        {
            var newspaper = await _newspaperService.GetNewspaperByNameAsync(name);
            
            if (newspaper == null)
                return NotFound($"Newspaper with name '{name}' not found");

            var newspaperDto = _mapper.Map<NewspaperDto>(newspaper);
            return Ok(newspaperDto);
        }

        [HttpPost]
        public async Task<ActionResult<NewspaperDto>> CreateNewspaper(CreateNewspaperDto createNewspaperDto)
        {
            try
            {
                var newspaper = _mapper.Map<Core.Domain.Models.Newspaper>(createNewspaperDto);
                var createdNewspaper = await _newspaperService.CreateNewspaperAsync(newspaper);
                var newspaperDto = _mapper.Map<NewspaperDto>(createdNewspaper);
                
                return CreatedAtAction(nameof(GetNewspaperById), new { id = newspaperDto.Id }, newspaperDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<NewspaperDto>> UpdateNewspaper(int id, UpdateNewspaperDto updateNewspaperDto)
        {
            try
            {
                var newspaper = _mapper.Map<Core.Domain.Models.Newspaper>(updateNewspaperDto);
                newspaper.Id = id;
                
                var updatedNewspaper = await _newspaperService.UpdateNewspaperAsync(newspaper);
                var newspaperDto = _mapper.Map<NewspaperDto>(updatedNewspaper);
                
                return Ok(newspaperDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNewspaper(int id)
        {
            try
            {
                var result = await _newspaperService.DeleteNewspaperAsync(id);
                if (result)
                {
                    return NoContent();
                }
                return NotFound($"Newspaper with ID {id} not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
} 