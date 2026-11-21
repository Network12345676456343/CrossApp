using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Configuration;
using Core.Domain;
using Core.Import;
using Core.Services;
using Core.Storage;
using Microsoft.Extensions.Configuration;

// 1. Збирання конфігурації (порядок джерел: appsettings -> appsettings.{Env} -> user-secrets -> env -> args)
var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();

var options = config.Get<AppOptions>() ?? new AppOptions();

// Підтримка змінної CROSSAPP_DATA_PATH (перекриває Storage:DataPath)
var legacyDataPath = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
var storageOptions = new StorageOptions
{
    DataPath = string.IsNullOrWhiteSpace(legacyDataPath) ? options.Storage.DataPath : legacyDataPath,
    FileName = options.Storage.FileName
};

// Отримуємо кросплатформний повний шлях відносно каталогу застосунку
var fullDataPath = storageOptions.ResolveFullPath(AppContext.BaseDirectory);

// Читаємо секрет (виводимо лише факт наявності та довжину)
var apiKey = config["Integration:ApiKey"];

Console.WriteLine($"Середовище : {envName} (Log: {options.Logging.MinimumLevel})");
Console.WriteLine($"Термін видачі (LoanDays) : {options.Library.LoanDays} дн.");
Console.WriteLine($"Файл даних : {fullDataPath}");
Console.WriteLine(string.IsNullOrEmpty(apiKey)
    ? "ApiKey     : не заданий"
    : $"ApiKey     : присутній, довжина {apiKey.Length}");
Console.WriteLine();

// 2. Ініціалізація файлового сховища та сервісу
var store = new FileLoanStore(fullDataPath);
var service = new LendingService(store);
var importer = new JsonCatalogImporter();

// 3. Розбір аргументів командного рядка (export / import / --file <шлях>)
var command = "import";
string? customFilePath = null;

for (var i = 0; i < args.Length; i++)
{
    if (args[i].Equals("export", StringComparison.OrdinalIgnoreCase))
    {
        command = "export";
    }
    else if (args[i].Equals("import", StringComparison.OrdinalIgnoreCase))
    {
        command = "import";
    }
    else if (args[i] == "--file" && i + 1 < args.Length)
    {
        customFilePath = args[++i];
    }
    else if (!args[i].StartsWith("--") && customFilePath is null)
    {
        customFilePath = args[i];
    }
}

using var userCts = new CancellationTokenSource();
using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    userCts.Cancel();
};

var progress = new Progress<ImportProgress>(p =>
{
    Console.Write($"\r[Прогрес] Опрацьовано записів: {p.ProcessedCount}...");
});

try
{
    if (command == "export")
    {
        // Якщо сховище порожнє, додаємо тестові записи для демонстрації експорту (можна прибрати за потреби)
        if (store.List().Count == 0)
        {
            var samplePath = Path.Combine(AppContext.BaseDirectory, "data", "sample.json");
            if (File.Exists(samplePath))
            {
                await service.ImportAsync(importer, samplePath, progress, linkedCts.Token);
                Console.WriteLine();
            }
        }

        var items = store.List();
        var targetExportPath = customFilePath is not null
            ? Path.GetFullPath(customFilePath)
            : fullDataPath;

        var exportDir = Path.GetDirectoryName(targetExportPath);
        if (!string.IsNullOrEmpty(exportDir))
            Directory.CreateDirectory(exportDir);

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var dtos = items.Select(l => l.ToDto()).ToList();
        var tmpFile = targetExportPath + ".tmp";
        await File.WriteAllTextAsync(tmpFile, JsonSerializer.Serialize(dtos, jsonOptions), linkedCts.Token);
        File.Move(tmpFile, targetExportPath, overwrite: true);

        Console.WriteLine($"Експортовано {items.Count} записів у файл: {targetExportPath}");
    }
    else
    {
        // Імпорт: беремо переданий шлях або дефолтний sample.json відносно AppContext.BaseDirectory
        var importSourcePath = customFilePath is not null
            ? (Path.IsPathRooted(customFilePath) ? customFilePath : Path.GetFullPath(customFilePath))
            : Path.Combine(AppContext.BaseDirectory, "data", "sample.json");

        var added = await service.ImportAsync(importer, importSourcePath, progress, linkedCts.Token);
        Console.WriteLine();
        Console.WriteLine($"Імпортовано записів: {added} (усього у сховищі: {store.List().Count})");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine();
    Console.WriteLine("Операцію скасовано.");
}
catch (DomainException ex)
{
    Console.WriteLine();
    Console.WriteLine($"Дані відхилено: {ex.Message}");
}
catch (InvalidDataException ex)
{
    Console.WriteLine();
    Console.WriteLine($"Помилка формату JSON: {ex.Message}");
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