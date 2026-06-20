namespace DocumentService.Domain.Enums
{
    public enum DocumentType
    {
        ComplianceReport = 0,
        InspectionReport = 1,
        RiskAssessment = 2,
        PropertySummary = 3,
        InsuranceReport = 4
    }

    public enum DocumentFormat
    {
        PDF = 0,
        Word = 1,
        Excel = 2,
        HTML = 3,
        JSON = 4
    }

    public enum ReportStatus
    {
        Pending = 0,
        Generating = 1,
        Completed = 2,
        Failed = 3
    }
}
