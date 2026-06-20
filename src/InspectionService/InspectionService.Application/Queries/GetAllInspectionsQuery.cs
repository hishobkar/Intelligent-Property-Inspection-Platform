using InspectionService.Domain.Entities;
using MediatR;

namespace InspectionService.Application.Queries
{
    public class GetAllInspectionsQuery : IRequest<IEnumerable<Inspection>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
