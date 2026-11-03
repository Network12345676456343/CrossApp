namespace CrossApp.Core.Reports;

public sealed record TopReaderLine(
    string ReaderId,
    int ActiveLoansCount);