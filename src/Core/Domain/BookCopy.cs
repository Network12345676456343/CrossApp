using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    public string Id { get; }
    public string Isbn { get; }
    public string Title { get; }
    public bool IsIssued { get; private set; }

    private BookCopy(string id, string isbn, string title, bool isIssued)
    {
        Id = id;
        Isbn = isbn;
        Title = title;
        IsIssued = isIssued;
    }

    public static BookCopy Create(string id, string isbn, string title, bool isIssued = false)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор примірника не може бути порожнім", nameof(id));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва книги не може бути порожньою", nameof(title));

        return new BookCopy(id.Trim(), isbn.Trim().ToUpperInvariant(), title.Trim(), isIssued);
    }

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