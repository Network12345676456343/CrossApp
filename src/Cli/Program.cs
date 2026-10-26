using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Шляхи: лог-файл створюється безпосередньо в кореневій папці logs/
string catalogPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
string logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "app.log");

var logDir = Path.GetDirectoryName(logPath);
if (!string.IsNullOrEmpty(logDir))
{
    Directory.CreateDirectory(logDir);
}

// 1. Composition Root (ручний DI та тернарний вибір сховища)
bool useFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);

ILoanStore store = useFile
    ? new FileLoanStore(catalogPath)
    : new InMemoryLoanStore(SampleData.Loans());

var service = new LendingService(store);

Console.WriteLine("=================================================");
Console.WriteLine($"Сховище: {store.GetType().Name}");
Console.WriteLine($"Шлях до логу: {logPath}");
Console.WriteLine("=================================================\n");

// 2. Підписник 1: локальний метод (пише в консоль і дописує у logs/app.log)
void OnLoanChanged(object? sender, LoanChangedEventArgs e)
{
    var statusText = e.Kind == LoanChangeKind.Issued
        ? $"видано читачу {e.ReaderId} до {e.DueUtc:yyyy-MM-dd}"
        : $"повернуто читачем {e.ReaderId}";

    var logLine = $"{e.At:HH:mm:ss} | {e.Kind,-8} | Запис: {e.LoanId} | Примірник: {e.BookCopyId} | {statusText}";

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"[ПОДІЯ]: {logLine}");
    Console.ResetColor();

    File.AppendAllText(logPath, logLine + Environment.NewLine);
}

service.Changed += OnLoanChanged;

// 3. Підписник 2: лямбда-вираз із замиканням над лічильником
int changesCount = 0;
service.Changed += (_, _) => changesCount++;

// --- Крок 1. Виконання двох операцій під час активної підписки ---
Console.WriteLine("--- Крок 1: Активна підписка (Консоль + Файл + Лічильник) ---");
var firstLoan = service.IssueCopy("BC-204", "READER-042");
service.ReturnCopy(firstLoan.Id);

Console.WriteLine($"\n[Лічильник] Подій отримано: {changesCount}");

// --- Крок 2. Відписка від події (-=) ---
Console.WriteLine("\n--- Крок 2: Відписка локального методу (-= OnLoanChanged) ---");
service.Changed -= OnLoanChanged;

// Операція тиха для консолі й файлу, але збільшує лічильник через замикання
var silentLoan = service.IssueCopy("BC-305", "READER-999");
Console.WriteLine($"[Лічильник після тихої операції] Подій отримано: {changesCount} (збільшився лише лічильник)");

// --- Крок 3. Сценарій відмови ---
Console.WriteLine("\n--- Крок 3: Сценарій відмови (помилка без виклику події) ---");
try
{
    service.ReturnCopy("UNKNOWN-ID");
}
catch (InvalidOperationException ex)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[Очікувана помилка перехоплена]: {ex.Message}");
    Console.ResetColor();
}

Console.WriteLine($"[Лічильник після винятку] Подій отримано: {changesCount} (кількість не змінилась)");

// --- Крок 4. Фільтрація через Func<Loan, bool> ---
Console.WriteLine("\n--- Крок 4: Фільтрація через Func<Loan, bool> ---");
var activeLoans = service.FindAll(l => l.Status == LoanStatus.Active);
foreach (var loan in activeLoans)
{
    Console.WriteLine($"  -> Активна позичка: {loan.Id} | Примірник: {loan.BookCopyId} | Читач: {loan.ReaderId}");
}

// -------------------------------------------------------------
// Додаткове завдання 1: Реалізації INotificationSink всередині Cli
// -------------------------------------------------------------
public sealed class ConsoleSink : INotificationSink
{
    public void Notify(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"[ConsoleSink]: {message}");
        Console.ResetColor();
    }
}

public sealed class FileSink(string filePath) : INotificationSink
{
    private readonly string _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

    public void Notify(string message)
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.AppendAllText(_filePath, $"{DateTime.UtcNow:HH:mm:ss} | {message}{Environment.NewLine}");
    }
}