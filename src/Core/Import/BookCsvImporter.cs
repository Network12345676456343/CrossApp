using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class BookCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<BookDto> Load(string path)
    {
        var items = new List<BookDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (lineNumber == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {lineNumber}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<BookDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);
        int currentYear = DateTime.UtcNow.Year;

        return parts switch
        {
            { Length: < 4 } => new ParseFailed($"очікую щонайменше 4 колонки, отримав {parts.Length}"),

            ["", _, _, ..] or [_, "", _, ..] or [_, _, "", ..]
                => new ParseFailed("Id, ISBN або назва не можуть бути порожніми"),

            [_, _, _, var yearStr, ..] when !int.TryParse(yearStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int y) || y < 1450 || y > currentYear
                => new ParseFailed($"рік видання '{yearStr}' некоректний або поза межами допустимого діапазону (1450–{currentYear})"),

            [var id, var isbn, var title, var yearStr]
                => new ParseOk(new BookDto(id, isbn, title, int.Parse(yearStr, CultureInfo.InvariantCulture))),

            [var id, var isbn, var title, var yearStr, var author]
                => new ParseOk(new BookDto(id, isbn, title, int.Parse(yearStr, CultureInfo.InvariantCulture), string.IsNullOrWhiteSpace(author) ? null : author)),

            _ => new ParseFailed($"занадто багато колонок: {parts.Length} (очікується 4 або 5)")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(BookDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}