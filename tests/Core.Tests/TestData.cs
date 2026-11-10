using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    public static List<Loan> Loans()
    {
        var copy1 = new BookCopy("BC-01", "978-01", "Чистий код");
        var copy2 = new BookCopy("BC-02", "978-02", "CLR via C#");
        var copy3 = new BookCopy("BC-03", "978-03", "Патерни GoF");

        return
        [
            Loan.Open("LN-01", copy1, "READER-01", DateTime.UtcNow.AddDays(-10)),
            Loan.Open("LN-02", copy2, "READER-01", DateTime.UtcNow.AddDays(-5)),
            Loan.Open("LN-03", copy3, "READER-02", DateTime.UtcNow.AddDays(-2))
        ];
    }
}