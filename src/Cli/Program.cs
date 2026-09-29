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

ImportResult<BookDto> result = BookCsvImporter.Load(path);

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
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
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string err in result.Errors)
    {
        Console.WriteLine($"  ! {err}");
    }
}

return 0;