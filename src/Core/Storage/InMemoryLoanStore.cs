namespace Core.Storage;

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions;
using Core.Domain;

public sealed class InMemoryLoanStore(IEnumerable<Loan>? seed = null) : ILoanStore
{
    private readonly Dictionary<string, Loan> _items =
        (seed ?? []).ToDictionary(l => l.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Loan> List() => _items.Values.ToList();

    public Loan? GetById(string id) => _items.GetValueOrDefault(id);

    public void Add(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        if (_items.ContainsKey(loan.Id))
            throw new InvalidOperationException($"Позичка з Id='{loan.Id}' уже існує в пам'яті.");

        _items.Add(loan.Id, loan);
    }

    public void Update(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        if (!_items.ContainsKey(loan.Id))
            throw new InvalidOperationException($"Позичку з Id='{loan.Id}' не знайдено для оновлення.");

        _items[loan.Id] = loan;
    }

    public bool Remove(string id) => _items.Remove(id);

    public IReadOnlyList<Loan> Find(Func<Loan, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _items.Values.Where(predicate).ToList();
    }
}