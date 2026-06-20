namespace PropertyService.Domain.Enums
{
    public enum PropertyType
    {
        Residential = 0,
        Commercial = 1,
        Industrial = 2,
        Land = 3,
        MixedUse = 4
    }

    public enum PropertyStatus
    {
        Available = 0,
        Sold = 1,
        UnderContract = 2,
        Rented = 3,
        UnderConstruction = 4,
        UnderInspection = 5
    }
}