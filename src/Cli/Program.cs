using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// 1. Composition Root: Ручний DI
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ILoanStore store = useFile
    ? new FileLoanStore(dataPath)
    : new InMemoryLoanStore(SampleData.Loans());

var service = new LendingService(store);

Console.WriteLine($"=================================================");
Console.WriteLine($"Сховище: {store.GetType().Name}");
Console.WriteLine($"=================================================\n");

// 2. Демонстрація операцій
try
{
    Console.WriteLine("[1] Створення нової видачі примірника книги...");
    var newLoan = service.IssueCopy("BC-204", "READER-042");
    Console.WriteLine($"  Створено видачу: {newLoan.Id} | Примірник: {newLoan.BookCopyId} -> Читач: {newLoan.ReaderId} | Стан: {newLoan.Status}");

    Console.WriteLine("\n[2] Повернення примірника (закриття видачі)...");
    service.ReturnCopy(newLoan.Id);
    Console.WriteLine($"  Видачу {newLoan.Id} успішно закрито.");

    Console.WriteLine("\n[3] Поточний список усіх видач у сховищі:");
    foreach (var l in service.All())
    {
        Console.WriteLine($"  [{l.Id}] Примірник: {l.BookCopyId,-8} | Читач: {l.ReaderId,-10} | Стан: {l.Status,-9} | Дата: {l.IssuedAtUtc:yyyy-MM-dd}");
    }

    // Демонстрація пошуку за предикатом Func<Loan, bool>
    Console.WriteLine("\n[*] Фільтрація: тільки активні видачі (через Func):");
    var activeLoans = service.Find(l => l.Status == LoanStatus.Active);
    foreach (var l in activeLoans)
    {
        Console.WriteLine($"  -> Активна позичка: {l.Id} для читача {l.ReaderId}");
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Помилка бізнес-логіки: {ex.Message}");
    Console.ResetColor();
}

// 3. Сценарій відмови
Console.WriteLine("\n-------------------------------------------------");
Console.WriteLine("[4] Перевірка обробки відмови (спроба закрити неіснуючий запис):");
try
{
    service.ReturnCopy("UNKNOWN-ID");
}
catch (InvalidOperationException ex)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"  [Очікувана помилка перехоплена]: {ex.Message}");
    Console.ResetColor();
}
Console.WriteLine("-------------------------------------------------");