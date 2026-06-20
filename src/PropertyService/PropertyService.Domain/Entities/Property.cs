using System.ComponentModel.DataAnnotations;
using PropertyService.Domain.Enums;

namespace PropertyService.Domain.Entities
{
    public class Property
    {
        public Guid Id { get; set; }
        public string PropertyReference { get; set; } = string.Empty;
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
        public PropertyStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}