namespace CrossApp.Core.Reports;

public sealed record GroupSummary(
    string Key,
    int Count,
    int TotalQuantity);