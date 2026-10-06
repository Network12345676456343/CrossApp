namespace Core.Domain;

public static class LoanPolicy
{
    public const int MaxActiveLoansPerReader = 5;

    /// <summary>
    /// Перевіряє інваріант, що охоплює дві сутності: читача та сукупність його видач.
    /// </summary>
    public static Loan IssueBook(
        string loanId,
        BookCopy copy,
        string readerId,
        IReadOnlyList<Loan> readerActiveLoans,
        DateTime issuedAtUtc)
    {
        if (readerActiveLoans.Count(l => l.ReaderId == readerId && l.Status == LoanStatus.Active) >= MaxActiveLoansPerReader)
        {
            throw new InvalidOperationException(
                $"Читач '{readerId}' уже має максимальну кількість активних видач ({MaxActiveLoansPerReader}). Нова видача неможлива");
        }

        return Loan.Open(loanId, copy, readerId, issuedAtUtc);
    }
}