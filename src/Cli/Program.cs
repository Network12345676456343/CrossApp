using System.Text;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Помилка: файл не знайдено за шляхом: {Path.GetFullPath(path)}");
    return 1;
}

string extension = Path.GetExtension(path).ToLowerInvariant();

// Додаткове завдання 1: вибір імпортера за розширенням через switch expression
ImportResult<BookDto> result = extension switch
{
    ".csv"  => BookCsvImporter.Load(path),
    ".json" => BookJsonImporter.Load(path),
    _ => throw new NotSupportedException($"Формат файлу '{extension}' не підтримується. Використовуйте .csv або .json")
};

Console.WriteLine($"Формат: {extension.ToUpperInvariant()} | Джерело: {path}");
Console.WriteLine(new string('-', 78));
Console.WriteLine($"  {"ID",-7} {"ISBN",-19} {"РІК",-6} {"НАЗВА",-28} {"АВТОР"}");
Console.WriteLine(new string('-', 78));

foreach (BookDto b in result.Items.Take(5))
{
    string author = b.Author ?? "—";
    Console.WriteLine($"  {b.Id,-7} {b.Isbn,-19} {b.Year,-6} {b.Title,-28} {author}");
}

Console.WriteLine(new string('-', 78));

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено записів: {result.Errors.Count}");
    foreach (string err in result.Errors)
    {
        Console.WriteLine($"  ! {err}");
    }
    Console.WriteLine(new string('-', 78));
}

// Додаткове завдання 3: статистика імпорту одним рядком
int total = result.Items.Count + result.Errors.Count;
double errorRate = total > 0 ? (double)result.Errors.Count / total * 100 : 0;
Console.WriteLine($"СТАТИСТИКА: Всього: {total} | Прийнято: {result.Items.Count} | Пропущено: {result.Errors.Count} | Помилок: {errorRate:F1}%");

return 0;