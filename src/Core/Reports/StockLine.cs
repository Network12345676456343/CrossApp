namespace CrossApp.Core.Reports;

public sealed record StockLine(
    string Id,
    string ReaderId,
    string BookCopyId,
    DateTime DueDate,
    int DaysOverdue);