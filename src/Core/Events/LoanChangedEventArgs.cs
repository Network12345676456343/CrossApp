namespace Core.Events;

using System;

public sealed class LoanChangedEventArgs(
    string loanId,
    string bookCopyId,
    string readerId,
    LoanChangeKind kind,
    DateTime dueUtc) : EventArgs
{
    public string LoanId { get; } = loanId;
    public string BookCopyId { get; } = bookCopyId;
    public string ReaderId { get; } = readerId;
    public LoanChangeKind Kind { get; } = kind;
    public DateTime DueUtc { get; } = dueUtc;
    public DateTimeOffset At { get; } = DateTimeOffset.UtcNow;
}