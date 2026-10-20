namespace Core.Domain;

using System;
using Core.Dto;

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

    public static Loan Open(
        string id,
        BookCopy copy,
        string readerId,
        DateTime issuedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(copy, "Примірник книги обов'язковий");

        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));

        if (issuedAtUtc > DateTime.UtcNow.AddMinutes(5))
            throw new ArgumentOutOfRangeException(
                nameof(issuedAtUtc),
                issuedAtUtc,
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
        ArgumentNullException.ThrowIfNull(copy);

        if (copy.Id != BookCopyId)
            throw new InvalidOperationException(
                $"Невідповідність примірника: видача прив'язана до {BookCopyId}, передано {copy.Id}");

        if (returnedAtUtc < IssuedAtUtc)
            throw new ArgumentOutOfRangeException(
                nameof(returnedAtUtc),
                returnedAtUtc,
                $"Дата повернення ({returnedAtUtc:yyyy-MM-dd HH:mm}) не може бути ранішою за дату видачі ({IssuedAtUtc:yyyy-MM-dd HH:mm})");

        switch (Status)
        {
            case LoanStatus.Active:
                break;

            case LoanStatus.Returned:
                throw new InvalidOperationException($"Видача '{Id}' уже була закрита раніше");

            case LoanStatus.Cancelled:
                throw new InvalidOperationException($"Видача '{Id}' скасована, її не можна завершити поверненням");

            default:
                throw new InvalidOperationException($"Невідомий стан видачі: {Status}");
        }

        ReturnedAtUtc = returnedAtUtc;
        Status = LoanStatus.Returned;
        copy.Return();
    }

    public void Cancel(BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);

        if (copy.Id != BookCopyId)
            throw new InvalidOperationException(
                $"Невідповідність примірника: видача прив'язана до {BookCopyId}, передано {copy.Id}");

        if (Status != LoanStatus.Active)
            throw new InvalidOperationException($"Неможливо скасувати видачу '{Id}' у стані {Status}");

        Status = LoanStatus.Cancelled;
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
        ArgumentNullException.ThrowIfNull(dto);

        if (!Enum.TryParse<LoanStatus>(dto.Status, true, out var status)
            || !Enum.IsDefined(typeof(LoanStatus), status))
        {
            throw new ArgumentException($"Невідомий статус у DTO: {dto.Status}", nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Id))
            throw new ArgumentException("Ідентифікатор у DTO обов'язковий", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.BookCopyId))
            throw new ArgumentException("Ідентифікатор книги у DTO обов'язковий", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.ReaderId))
            throw new ArgumentException("Ідентифікатор читача у DTO обов'язковий", nameof(dto));

        if (dto.ReturnedAtUtc.HasValue && dto.ReturnedAtUtc.Value < dto.IssuedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dto),
                "Дата повернення у DTO не може передувати даті видачі");
        }

        if (status == LoanStatus.Returned && !dto.ReturnedAtUtc.HasValue)
        {
            throw new ArgumentException(
                "Для поверненої видачі необхідно вказати дату повернення",
                nameof(dto));
        }

        if (status != LoanStatus.Returned && dto.ReturnedAtUtc.HasValue)
        {
            throw new ArgumentException(
                "Дата повернення може бути вказана лише для поверненої видачі",
                nameof(dto));
        }

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