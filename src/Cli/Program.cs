using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;
using Core.Import;
using Core.Services;
using Core.Storage;

// Підтримка формату виклику з методички: --file <шлях> або просто передача шляху
var dataPath = "data/sample.json";
for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--file" && i + 1 < args.Length)
    {
        dataPath = args[i + 1];
        break;
    }
    if (!args[i].StartsWith("--"))
    {
        dataPath = args[i];
        break;
    }
}

using var userCts = new CancellationTokenSource();
// Для демонстрації скасування через таймаут:
//using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));
using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    userCts.Cancel();
};

var store = new InMemoryLoanStore();
var service = new LendingService(store);
var importer = new JsonCatalogImporter();

var progress = new Progress<ImportProgress>(p =>
{
    Console.Write($"\r[Прогрес] Опрацьовано записів: {p.ProcessedCount}...");
});

try
{
    var added = await service.ImportAsync(importer, dataPath, progress, linkedCts.Token);
    Console.WriteLine();
    Console.WriteLine($"Імпортовано записів: {added}");
}
catch (OperationCanceledException)
{
    Console.WriteLine();
    Console.WriteLine("Імпорт скасовано.");
}
catch (DomainException ex)
{
    Console.WriteLine();
    Console.WriteLine($"Дані відхилено: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine();
    Console.WriteLine($"Файл не знайдено: {ex.FileName}");
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine($"Технічна помилка: {ex.Message}");
}