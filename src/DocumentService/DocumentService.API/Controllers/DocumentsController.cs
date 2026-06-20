using MediatR;
using Microsoft.AspNetCore.Mvc;
using DocumentService.Application.Commands;
using DocumentService.Application.Queries;
using DocumentService.Domain.Entities;
using System.Net.Mime;

namespace DocumentService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class DocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IMediator mediator, ILogger<DocumentsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("compliance-report")]
        [ProducesResponseType(typeof(Document), StatusCodes.Status201Created)]
        public async Task<IActionResult> GenerateComplianceReport([FromBody] GenerateComplianceReportCommand command)
        {
            try
            {
                var document = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating compliance report");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Document), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentById(Guid id)
        {
            var query = new GetDocumentByIdQuery { Id = id };
            var document = await _mediator.Send(query);

            if (document == null)
                return NotFound();

            return Ok(document);
        }

        [HttpGet("property/{propertyId}")]
        [ProducesResponseType(typeof(IEnumerable<Document>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocumentsByProperty(Guid propertyId)
        {
            var query = new GetDocumentsByPropertyQuery { PropertyId = propertyId };
            var documents = await _mediator.Send(query);
            return Ok(documents);
        }

        [HttpGet("{id}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadDocument(Guid id)
        {
            var command = new DownloadDocumentCommand { DocumentId = id };
            var result = await _mediator.Send(command);

            if (result == null)
                return NotFound();

            return File(result.Stream, result.ContentType, result.FileName);
        }

        [HttpPost("generate-inspection-report")]
        [ProducesResponseType(typeof(Document), StatusCodes.Status201Created)]
        public async Task<IActionResult> GenerateInspectionReport([FromBody] GenerateInspectionReportCommand command)
        {
            try
            {
                var document = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating inspection report");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}