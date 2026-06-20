using MediatR;
using PropertyService.Domain.Entities;

namespace PropertyService.Application.Queries
{
    public class GetAllPropertiesQuery : IRequest<IEnumerable<Property>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
