using System;
using System.Collections.Generic;
using System.Threading;
using Core.Domain;

namespace Core.Abstractions;

public sealed record ImportProgress(int ProcessedCount, int TotalCount, int Percentage);

public interface ICatalogImporter
{
    IAsyncEnumerable<BookCopy> ReadCopiesAsync(
        string path,
        IProgress<ImportProgress>? progress = null,
        CancellationToken ct = default);
}