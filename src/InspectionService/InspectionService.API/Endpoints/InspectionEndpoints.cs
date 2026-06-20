using InspectionService.Application.Commands;
using InspectionService.Application.Queries;
using InspectionService.Domain.Entities;
using MediatR;

namespace InspectionService.API.Endpoints
{
    public static class InspectionEndpoints
    {
        public static void MapInspectionEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/inspections").WithTags("Inspections");

            group.MapPost("/", async Task<IResult> (CreateInspectionCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Created($"/api/inspections/{result.Id}", result);
            })
            .WithName("CreateInspection")
            .Produces<Inspection>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/{id:guid}", async Task<IResult> (Guid id, IMediator mediator) =>
            {
                var query = new GetInspectionByIdQuery { Id = id };
                var result = await mediator.Send(query);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetInspectionById")
            .Produces<Inspection>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/property/{propertyId:guid}", async Task<IResult> (Guid propertyId, IMediator mediator) =>
            {
                var query = new GetInspectionsByPropertyQuery { PropertyId = propertyId };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetInspectionsByProperty")
            .Produces<IEnumerable<Inspection>>(StatusCodes.Status200OK);

            group.MapPost("/{id:guid}/complete", async Task<IResult> (Guid id, CompleteInspectionCommand body, IMediator mediator) =>
            {
                body.InspectionId = id;
                var result = await mediator.Send(body);
                return result ? Results.Ok(new { message = "Inspection completed" }) : Results.NotFound();
            })
            .WithName("CompleteInspection")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/", async Task<IResult> ([Microsoft.AspNetCore.Mvc.FromQuery] int page = 1, [Microsoft.AspNetCore.Mvc.FromQuery] int pageSize = 10, IMediator mediator = default!) =>
            {
                var query = new GetAllInspectionsQuery { Page = page, PageSize = pageSize };
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetAllInspections")
            .Produces<IEnumerable<Inspection>>(StatusCodes.Status200OK);
        }
    }
}
