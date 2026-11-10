namespace Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;
using Core.Events;

public sealed class LendingService(ILoanStore store)
{
    private readonly ILoanStore _store = store ?? throw new ArgumentNullException(nameof(store));

    // Подія зміни стану за стандартом .NET EventHandler<TEventArgs>
    public event EventHandler<LoanChangedEventArgs>? Changed;

    // Приватний метод безпечного підняття події
    private void OnChanged(Loan loan, LoanChangeKind kind)
    {
        Changed?.Invoke(this, new LoanChangedEventArgs(
            loan.Id,
            loan.BookCopyId,
            loan.ReaderId,
            kind,
            loan.IssuedAtUtc.AddDays(14)
        ));
    }

    public Loan IssueCopy(string bookCopyId, string readerId)
    {
        var id = "LN-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var copy = new BookCopy(bookCopyId);
        var loan = Loan.Open(id, copy, readerId, DateTime.UtcNow);

        _store.Add(loan);

        // Підняття події ВИКЛЮЧНО після успішного збереження в сховище
        OnChanged(loan, LoanChangeKind.Issued);
        return loan;
    }

    public void ReturnCopy(string loanId)
    {
        var loan = _store.GetById(loanId) 
            ?? throw new DomainException($"Позичку з Id='{loanId}' не знайдено.");

        var copy = new BookCopy(loan.BookCopyId, isIssued: true);
        loan.Close(copy, DateTime.UtcNow);

        _store.Update(loan);

        // Підняття події ВИКЛЮЧНО після успішного оновлення
        OnChanged(loan, LoanChangeKind.Returned);
    }

    /// <summary>
    /// Асинхронний потоковий імпорт примірників книг із підтримкою скасування та прогресу.
    /// </summary>
    public async Task<int> ImportAsync(
        ICatalogImporter importer,
        string filePath,
        IProgress<ImportProgress>? progress = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(importer);

        var addedCount = 0;

        await foreach (var copy in importer.ReadCopiesAsync(filePath, progress, ct))
        {
            ct.ThrowIfCancellationRequested();

            // Ініціалізуємо базову позичку в системі для імпортованого примірника (якщо потрібна реєстрація в ILoanStore)
            var loanId = "LN-IMP-" + copy.Id;

            if (_store.GetById(loanId) is not null)
            {
                continue; // Ідемпотентність: уникаємо дублікатів
            }

            var loan = Loan.Open(loanId, copy, "SYSTEM_IMPORT", DateTime.UtcNow);
            _store.Add(loan);

            OnChanged(loan, LoanChangeKind.Issued);
            addedCount++;
        }

        return addedCount;
    }

    public IReadOnlyList<Loan> All() => _store.List();

    public Loan? Find(string id) => _store.GetById(id);

    // Метод пошуку з Func-предикатом (повертає результат виклику)
    public IReadOnlyList<Loan> FindAll(Func<Loan, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }
}