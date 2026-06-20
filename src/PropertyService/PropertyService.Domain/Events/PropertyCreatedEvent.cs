namespace PropertyService.Domain.Events
{
    public class PropertyCreatedEvent
    {
        public Guid PropertyId { get; set; }
        public string PropertyReference { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public decimal EstimatedValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PropertyUpdatedEvent
    {
        public Guid PropertyId { get; set; }
        public string PropertyReference { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    public class PropertyDeletedEvent
    {
        public Guid PropertyId { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
