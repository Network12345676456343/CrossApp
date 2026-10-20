namespace Core.Storage;

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions;
using Core.Domain;

public sealed class CachingLoanStore(ILoanStore innerStore) : ILoanStore
{
    private readonly ILoanStore _innerStore = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
    private readonly Dictionary<string, Loan> _memoryCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _isCacheLoaded;

    public IReadOnlyList<Loan> List()
    {
        EnsureCache();
        return _memoryCache.Values.ToList();
    }

    public Loan? GetById(string id)
    {
        EnsureCache();
        return _memoryCache.GetValueOrDefault(id);
    }

    public void Add(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        _innerStore.Add(loan);
        _memoryCache[loan.Id] = loan;
    }

    public void Update(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        _innerStore.Update(loan);
        _memoryCache[loan.Id] = loan;
    }

    public bool Remove(string id)
    {
        var removed = _innerStore.Remove(id);
        if (removed)
        {
            _memoryCache.Remove(id);
        }
        return removed;
    }

    public IReadOnlyList<Loan> Find(Func<Loan, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        EnsureCache();
        return _memoryCache.Values.Where(predicate).ToList();
    }

    private void EnsureCache()
    {
        if (_isCacheLoaded) return;

        foreach (var item in _innerStore.List())
        {
            _memoryCache[item.Id] = item;
        }

        _isCacheLoaded = true;
    }
}