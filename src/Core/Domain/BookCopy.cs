using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    public string Id { get; }
    public string Isbn { get; }
    public string Title { get; }
    public bool IsIssued { get; private set; }

    // Публічний конструктор: дозволяє виклик як new BookCopy("BC-101"),
    // так і передачу повних даних, не ламаючи валідацію
    public BookCopy(string id, string isbn = "978-0-00-000000-0", string title = "Без назви", bool isIssued = false)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор примірника не може бути порожнім", nameof(id));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва книги не може бути порожньою", nameof(title));

        Id = id.Trim();
        Isbn = isbn.Trim().ToUpperInvariant();
        Title = title.Trim();
        IsIssued = isIssued;
    }

    public static BookCopy Create(string id, string isbn, string title, bool isIssued = false) =>
        new(id, isbn, title, isIssued);

    public void Issue()
    {
        if (IsIssued)
            throw new InvalidOperationException($"Примірник '{Id}' ({Title}) уже виданий, повторна видача неможлива");

        IsIssued = true;
    }

    public void Return()
    {
        if (!IsIssued)
            throw new InvalidOperationException($"Примірник '{Id}' ({Title}) не виданий, тому його не можна повернути");

        IsIssued = false;
    }

    public BookCopyDto ToDto() => new(Id, Isbn, Title, IsIssued);

    public static BookCopy FromDto(BookCopyDto dto) =>
        Create(dto.Id, dto.Isbn, dto.Title, dto.IsIssued);

    public override string ToString() =>
        $"{Id} [ISBN: {Isbn}] \"{Title}\" — {(IsIssued ? "ВИДАНИЙ" : "В НАЯВНОСТІ")}";
}