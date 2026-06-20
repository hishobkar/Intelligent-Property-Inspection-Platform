using MediatR;
using Microsoft.AspNetCore.Mvc;
using PropertyService.Application.Commands;
using PropertyService.Application.Queries;
using PropertyService.Domain.Entities;
using System.Net.Mime;

namespace PropertyService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class PropertiesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PropertiesController> _logger;

        public PropertiesController(IMediator mediator, ILogger<PropertiesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Property), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyCommand command)
        {
            try
            {
                var property = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetPropertyById), new { id = property.Id }, property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating property");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Property), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPropertyById(Guid id)
        {
            var query = new GetPropertyByIdQuery { Id = id };
            var property = await _mediator.Send(query);

            if (property == null)
                return NotFound();

            return Ok(property);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Property>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProperties([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllPropertiesQuery { Page = page, PageSize = pageSize };
            var properties = await _mediator.Send(query);
            return Ok(properties);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Property), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProperty(Guid id, [FromBody] UpdatePropertyCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID mismatch");

            var property = await _mediator.Send(command);
            if (property == null)
                return NotFound();

            return Ok(property);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProperty(Guid id)
        {
            var command = new DeletePropertyCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("owner/{ownerId}")]
        [ProducesResponseType(typeof(IEnumerable<Property>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPropertiesByOwner(string ownerId)
        {
            var query = new GetPropertiesByOwnerQuery { OwnerId = ownerId };
            var properties = await _mediator.Send(query);
            return Ok(properties);
        }
    }
}