namespace InspectionService.Domain.Enums
{
    public enum InspectionStatus
    {
        Scheduled = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3,
        Failed = 4
    }

    public enum InspectionType
    {
        Routine = 0,
        PrePurchase = 1,
        Annual = 2,
        Insurance = 3,
        Compliance = 4,
        Emergency = 5
    }

    public enum InspectionPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
}
