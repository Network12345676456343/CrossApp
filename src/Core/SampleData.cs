namespace Core;

using Core.Domain;

public static class SampleData
{
    public static IReadOnlyList<Loan> Loans() =>
    [
        Loan.Open("LN-001", new BookCopy("BC-101"), "RDR-01", DateTime.UtcNow.AddDays(-10)),
        Loan.Open("LN-002", new BookCopy("BC-102"), "RDR-02", DateTime.UtcNow.AddDays(-5)),
        Loan.Open("LN-003", new BookCopy("BC-103"), "RDR-03", DateTime.UtcNow.AddDays(-2))
    ];
}