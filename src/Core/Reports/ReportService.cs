namespace CrossApp.Core.Reports;

public static class ReportService
{
    // 1) Фільтр + сортування + проєкція
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<StockLine> source, DateTime thresholdDate)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .Where(l => l.DueDate < thresholdDate)
            .OrderBy(l => l.DueDate)
            .ThenBy(l => l.Id, StringComparer.Ordinal)
            .Select(l => new StockLine(
                l.Id,
                l.ReaderId,
                l.BookCopyId,
                l.DueDate,
                (thresholdDate - l.DueDate).Days))
            .ToList();
    }

    // 2) Топ-N зі стабільним порядком сортування (ThenBy)
    public static IReadOnlyList<TopReaderLine> TopByQuantity(IEnumerable<StockLine> source, int n)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(l => l.ReaderId, StringComparer.OrdinalIgnoreCase)
            .Select(g => new TopReaderLine(g.Key, g.Count()))
            .OrderByDescending(r => r.ActiveLoansCount)
            .ThenBy(r => r.ReaderId, StringComparer.Ordinal)
            .Take(n)
            .ToList();
    }

    // 3) Групування
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<StockLine> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(l => l.ReaderId, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(
                Key: g.Key,
                Count: g.Count(),
                TotalQuantity: g.Count()))
            .OrderByDescending(s => s.Count)
            .ThenBy(s => s.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // Додаткове: синтаксис запитів (Query Syntax)
    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<StockLine> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var query = from item in source
                    group item by item.ReaderId into g
                    let total = g.Count()
                    orderby total descending, g.Key
                    select new GroupSummary(g.Key, total, total);

        return query.ToList();
    }

    // 4) Агрегати з безпечною обробкою порожньої послідовності
    public static (int Items, int TotalOverdueDays, double AverageDaysRemaining, int MaxOverdueDays) Totals(
        IEnumerable<StockLine> source,
        DateTime asOfDate)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = source.ToList();
        if (list.Count == 0)
        {
            return (0, 0, 0d, 0);
        }

        var overdue = list.Where(l => l.DueDate < asOfDate).ToList();

        return (
            Items: list.Count,
            TotalOverdueDays: overdue.Sum(l => (asOfDate - l.DueDate).Days),
            AverageDaysRemaining: list.Average(l => (l.DueDate - asOfDate).TotalDays),
            MaxOverdueDays: overdue.Count == 0 ? 0 : overdue.Max(l => (asOfDate - l.DueDate).Days)
        );
    }

    // Додаткове: Join двох колекцій
    public static IReadOnlyList<BookLoanJoinLine> ActiveLoansJoin(
        IEnumerable<StockLine> loans,
        IEnumerable<string> priorityReaders)
    {
        ArgumentNullException.ThrowIfNull(loans);
        ArgumentNullException.ThrowIfNull(priorityReaders);

        return loans
            .Join(
                priorityReaders,
                loan => loan.ReaderId,
                reader => reader,
                (loan, reader) => new BookLoanJoinLine(
                    loan.Id,
                    reader,
                    loan.BookCopyId,
                    loan.DueDate,
                    "Active"),
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.LoanId, StringComparer.Ordinal)
            .ToList();
    }
}