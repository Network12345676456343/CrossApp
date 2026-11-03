namespace CrossApp.Core.Reports;

public sealed record BookLoanJoinLine(
    string LoanId,
    string ReaderId,
    string BookCopyId,
    DateTime DueDate,
    string Status);