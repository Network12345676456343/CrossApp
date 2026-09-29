using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class BookJsonImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<BookDto> Load(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            var parsed = JsonSerializer.Deserialize<List<BookDto>>(json, JsonOptions) ?? [];

            var items = new List<BookDto>();
            var errors = new List<string>();

            int currentYear = DateTime.UtcNow.Year;

            for (int i = 0; i < parsed.Count; i++)
            {
                int index = i + 1;
                BookDto b = parsed[i];

                // Валідація об'єктів з JSON аналогічно до CSV
                if (string.IsNullOrWhiteSpace(b.Id) || string.IsNullOrWhiteSpace(b.Isbn) || string.IsNullOrWhiteSpace(b.Title))
                {
                    errors.Add($"об'єкт #{index}: Id, ISBN або назва не можуть бути порожніми");
                }
                else if (b.Year < 1450 || b.Year > currentYear)
                {
                    errors.Add($"об'єкт #{index}: рік видання '{b.Year}' поза межами допустимого (1450–{currentYear})");
                }
                else
                {
                    items.Add(b);
                }
            }

            return new ImportResult<BookDto>(items, errors);
        }
        catch (JsonException ex)
        {
            return new ImportResult<BookDto>([], [$"Помилка структури JSON: {ex.Message}"]);
        }
    }
}