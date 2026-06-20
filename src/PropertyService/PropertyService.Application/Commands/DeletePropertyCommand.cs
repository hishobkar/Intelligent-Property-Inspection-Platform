using MediatR;

namespace PropertyService.Application.Commands
{
    public class DeletePropertyCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
}
