namespace CrossApp.Core.Reports;

public static class LinqReportExtensions
{
    public static IEnumerable<TSource> TopBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        return source
            .OrderByDescending(keySelector)
            .Take(count);
    }
}