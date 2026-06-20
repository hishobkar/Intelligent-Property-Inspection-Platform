using InspectionService.Domain.Entities;
using MediatR;

namespace InspectionService.Application.Queries
{
    public class GetInspectionsByPropertyQuery : IRequest<IEnumerable<Inspection>>
    {
        public Guid PropertyId { get; set; }
    }
}
