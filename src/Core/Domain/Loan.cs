using Core.Dto;

namespace Core.Domain;

public sealed class Loan
{
    public string Id { get; }
    public string BookCopyId { get; }
    public string ReaderId { get; }
    public DateTime IssuedAtUtc { get; }
    public DateTime? ReturnedAtUtc { get; private set; }
    public LoanStatus Status { get; private set; }

    private Loan(
        string id,
        string bookCopyId,
        string readerId,
        DateTime issuedAtUtc,
        DateTime? returnedAtUtc,
        LoanStatus status)
    {
        Id = id;
        BookCopyId = bookCopyId;
        ReaderId = readerId;
        IssuedAtUtc = issuedAtUtc;
        ReturnedAtUtc = returnedAtUtc;
        Status = status;
    }

    public static Loan Open(string id, BookCopy copy, string readerId, DateTime issuedAtUtc)
    {
        if (copy is null)
            throw new ArgumentNullException(nameof(copy), "Примірник книги обов'язковий");

        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));

        if (issuedAtUtc > DateTime.UtcNow.AddMinutes(5))
            throw new ArgumentOutOfRangeException(nameof(issuedAtUtc), issuedAtUtc,
                "Дата видачі не може належати майбутньому часу");

        copy.Issue();

        return new Loan(
            id.Trim(),
            copy.Id,
            readerId.Trim(),
            issuedAtUtc,
            null,
            LoanStatus.Active);
    }

    public void Close(BookCopy copy, DateTime returnedAtUtc)
    {
        if (copy is null)
            throw new ArgumentNullException(nameof(copy));

        if (copy.Id != BookCopyId)
            throw new InvalidOperationException($"Невідповідність примірника: видача прив'язана до {BookCopyId}, передано {copy.Id}");

        Status = Status switch
        {
            LoanStatus.Active => LoanStatus.Returned,
            LoanStatus.Returned => throw new InvalidOperationException($"Видача '{Id}' уже була закрита раніше"),
            LoanStatus.Cancelled => throw new InvalidOperationException($"Видача '{Id}' скасована, її не можна завершити поверненням"),
            _ => throw new InvalidOperationException($"Невідомий стан видачі: {Status}")
        };

        if (returnedAtUtc < IssuedAtUtc)
            throw new ArgumentOutOfRangeException(
                nameof(returnedAtUtc),
                returnedAtUtc,
                $"Дата повернення ({returnedAtUtc:yyyy-MM-dd HH:mm}) не може бути ранішою за дату видачі ({IssuedAtUtc:yyyy-MM-dd HH:mm})");

        ReturnedAtUtc = returnedAtUtc;
        copy.Return();
    }

    public void Cancel(BookCopy copy)
    {
        if (copy is null)
            throw new ArgumentNullException(nameof(copy));

        Status = Status switch
        {
            LoanStatus.Active => LoanStatus.Cancelled,
            _ => throw new InvalidOperationException($"Неможливо скасувати видачу '{Id}' у стані {Status}")
        };

        copy.Return();
    }

    public LoanDto ToDto() => new(
        Id,
        BookCopyId,
        ReaderId,
        IssuedAtUtc,
        ReturnedAtUtc,
        Status.ToString());

    public static Loan FromDto(LoanDto dto)
    {
        if (!Enum.TryParse<LoanStatus>(dto.Status, true, out var status))
            throw new ArgumentException($"Невідомий статус у DTO: {dto.Status}", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.Id))
            throw new ArgumentException("Ідентифікатор у DTO обов'язковий", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.BookCopyId))
            throw new ArgumentException("Ідентифікатор книги у DTO обов'язковий", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.ReaderId))
            throw new ArgumentException("Ідентифікатор читача у DTO обов'язковий", nameof(dto));

        if (dto.ReturnedAtUtc.HasValue && dto.ReturnedAtUtc < dto.IssuedAtUtc)
            throw new ArgumentOutOfRangeException(nameof(dto),
                "Дата повернення у DTO не може передувати даті видачі");

        return new Loan(
            dto.Id.Trim(),
            dto.BookCopyId.Trim(),
            dto.ReaderId.Trim(),
            dto.IssuedAtUtc,
            dto.ReturnedAtUtc,
            status);
    }

    public override string ToString() =>
        $"Видача {Id}: Книга [{BookCopyId}], Читач [{ReaderId}], Стан: {Status} (Видано: {IssuedAtUtc:dd.MM.yyyy HH:mm})";
}