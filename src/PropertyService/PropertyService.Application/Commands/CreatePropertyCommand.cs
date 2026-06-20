using MediatR;
using PropertyService.Domain.Entities;
using PropertyService.Domain.Enums;

namespace PropertyService.Application.Commands
{
    public class CreatePropertyCommand : IRequest<Property>
    {
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public PropertyType Type { get; set; }
        public int YearBuilt { get; set; }
        public decimal SquareFootage { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public decimal EstimatedValue { get; set; }
        public string OwnerId { get; set; } = string.Empty;
    }
}