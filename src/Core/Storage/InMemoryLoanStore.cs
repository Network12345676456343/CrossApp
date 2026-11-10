using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryLoanStore : ILoanStore
{
    private readonly ConcurrentDictionary<string, Loan> _loans = new();

    public void Add(Loan loan)
    {
        _loans[loan.Id] = loan;
    }

    public void Update(Loan loan)
    {
        _loans[loan.Id] = loan;
    }

    public Loan? GetById(string id)
    {
        _loans.TryGetValue(id, out var loan);
        return loan;
    }

    public IReadOnlyList<Loan> List() => _loans.Values.ToList();

    public bool Remove(string id)
    {
        return _loans.TryRemove(id, out _);
    }

    public IReadOnlyList<Loan> Find(Func<Loan, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _loans.Values.Where(predicate).ToList();
    }
}