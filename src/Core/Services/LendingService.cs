namespace Core.Services;

using System;
using System.Collections.Generic;
using Core.Abstractions;
using Core.Domain;

public sealed class LendingService(ILoanStore store)
{
    private readonly ILoanStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public Loan IssueCopy(string bookCopyId, string readerId)
    {
        var id = "LN-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var copy = new BookCopy(bookCopyId);
        var loan = Loan.Open(id, copy, readerId, DateTime.UtcNow);

        _store.Add(loan);
        return loan;
    }

    public void ReturnCopy(string loanId)
    {
        var loan = _store.GetById(loanId) 
            ?? throw new InvalidOperationException($"Позичку з Id='{loanId}' не знайдено.");

        var copy = new BookCopy(loan.BookCopyId, isIssued: true);
        loan.Close(copy, DateTime.UtcNow);

        _store.Update(loan);
    }

    public IReadOnlyList<Loan> All() => _store.List();

    public Loan? Find(string id) => _store.GetById(id);

    public IReadOnlyList<Loan> Find(Func<Loan, bool> predicate) => _store.Find(predicate);
}