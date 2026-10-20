namespace Core.Abstractions;

using System;
using System.Collections.Generic;
using Core.Domain;

public interface ILoanStore
{
    IReadOnlyList<Loan> List();
    Loan? GetById(string id);
    void Add(Loan loan);
    void Update(Loan loan);
    bool Remove(string id);

    // Додатковий контракт: пошук за предикатом (підготовка до тижнів 6-7)
    IReadOnlyList<Loan> Find(Func<Loan, bool> predicate);
}