using InspectionService.Domain.Entities;
using MediatR;

namespace InspectionService.Application.Queries
{
    public class GetInspectionByIdQuery : IRequest<Inspection?>
    {
        public Guid Id { get; set; }
    }
}
