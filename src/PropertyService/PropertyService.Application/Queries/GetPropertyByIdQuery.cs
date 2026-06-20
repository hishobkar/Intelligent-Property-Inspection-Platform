using MediatR;
using PropertyService.Domain.Entities;

namespace PropertyService.Application.Queries
{
    public class GetPropertyByIdQuery : IRequest<Property?>
    {
        public Guid Id { get; set; }
    }
}
