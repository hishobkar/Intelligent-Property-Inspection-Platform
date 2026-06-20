using MediatR;
using PropertyService.Domain.Entities;

namespace PropertyService.Application.Queries
{
    public class GetPropertiesByOwnerQuery : IRequest<IEnumerable<Property>>
    {
        public string OwnerId { get; set; } = string.Empty;
    }
}
