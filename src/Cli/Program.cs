using Core.Domain;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== СЦЕНАРІЙ 1: Успішна робота з доменною моделлю ===");

var copy = BookCopy.Create("BC-101", "978-0-13-235088-4", "Clean Code", isIssued: false);
Console.WriteLine($"Створено примірник: {copy}");

var loanTime = DateTime.UtcNow.AddHours(-2);
var loan = Loan.Open("LN-001", copy, "READER-42", loanTime);
Console.WriteLine($"Відкрито видачу: {loan}");
Console.WriteLine($"Стан примірника після видачі: {copy}");

loan.Close(copy, DateTime.UtcNow);
Console.WriteLine($"Закрито видачу: {loan}");
Console.WriteLine($"Стан примірника після повернення: {copy}");

Console.WriteLine();
Console.WriteLine("=== СЦЕНАРІЙ 2: Базові інваріанти сутностей ===");

TryDo("Порожній ISBN", () => BookCopy.Create("BC-102", "   ", "Refactoring"));

var busyCopy = BookCopy.Create("BC-200", "978-0-201-48567-7", "DDD", isIssued: false);
Loan.Open("LN-002", busyCopy, "READER-01", DateTime.UtcNow);
TryDo("Повторна видача вже виданого примірника", () => Loan.Open("LN-003", busyCopy, "READER-02", DateTime.UtcNow));

var validCopy = BookCopy.Create("BC-300", "978-0-321-12521-7", "TDD", isIssued: false);
var activeLoan = Loan.Open("LN-004", validCopy, "READER-05", DateTime.UtcNow);
TryDo("Дата повернення раніша за дату видачі", () => activeLoan.Close(validCopy, DateTime.UtcNow.AddDays(-1)));

Console.WriteLine();
Console.WriteLine("=== ДОДАТКОВЕ ЗАВДАННЯ 3: Перевірка переходів станів (switch expression) ===");

var stateCopy = BookCopy.Create("BC-400", "978-0-13-449416-6", "Clean Architecture", isIssued: false);
var loanForState = Loan.Open("LN-005", stateCopy, "READER-10", DateTime.UtcNow.AddHours(-1));
loanForState.Close(stateCopy, DateTime.UtcNow);

TryDo("Повторне закриття вже закритої видачі", () => loanForState.Close(stateCopy, DateTime.UtcNow));
TryDo("Скасування вже закритої видачі", () => loanForState.Cancel(stateCopy));

Console.WriteLine();
Console.WriteLine("=== ДОДАТКОВЕ ЗАВДАННЯ 2: Інваріант двох сутностей (ліміт 5 видач) ===");

var existingLoans = new List<Loan>();
for (int i = 1; i <= 5; i++)
{
    var c = BookCopy.Create($"BC-LIMIT-{i}", $"ISBN-{i}", $"Book {i}");
    existingLoans.Add(Loan.Open($"LN-EXIST-{i}", c, "READER-LIMIT", DateTime.UtcNow));
}

var extraCopy = BookCopy.Create("BC-LIMIT-6", "ISBN-6", "Book 6");
TryDo("Спроба взяти 6-ту книгу читачем з 5 активними видачами", () =>
{
    LoanPolicy.IssueBook("LN-LIMIT-6", extraCopy, "READER-LIMIT", existingLoans, DateTime.UtcNow);
});

Console.WriteLine();
Console.WriteLine("=== ДОДАТКОВЕ ЗАВДАННЯ 1: Зв'язок з ImportResult (дані + помилки) ===");

var simulatedImport = new ImportResult<BookCopyDto>(
    Items: new List<BookCopyDto>
    {
        new("BC-501", "978-1-111-11111-1", "Design Patterns", false),
        new("BC-502", "", "Book without ISBN", false), // Помилка інваріанту
        new("BC-503", "978-2-222-22222-2", "Domain-Driven Design Distilled", false)
    },
    Errors: new List<string> { "Рядок 4: файл пошкоджено, пропущено кому" } // Помилка парсингу файлу
);

var processed = simulatedImport.ToDomainEntities();

Console.WriteLine($"Успішно створено доменних сутностей: {processed.ValidEntities.Count}");
foreach (var item in processed.ValidEntities)
    Console.WriteLine($"  [OK] {item}");

Console.WriteLine($"Помилки формату файлу (з тижня 3): {processed.ParsingErrors.Count}");
foreach (var err in processed.ParsingErrors)
    Console.WriteLine($"  [FILE ERROR] {err}");

Console.WriteLine($"Помилки доменних інваріантів: {processed.InvariantErrors.Count}");
foreach (var (failedDto, err) in processed.InvariantErrors)
    Console.WriteLine($"  [DOMAIN ERROR] Id='{failedDto.Id}': {err}");

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [ПОМИЛКА ТЕСТУ] {title}: виняток НЕ виник!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [СПРАЦЮВАВ ІНВАРІАНТ] {title} -> {ex.GetType().Name}: {ex.Message}");
    }
}