using System.Collections.Generic;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class LendingServiceTests
{
    [Fact]
    // 1) Інваріант: відмова при спробі повторно видати виданий примірник
    public void Issue_AlreadyIssuedCopy_ThrowsDomainException_AndKeepsState()
    {
        var copy = new BookCopy("BC-101", "978-111", "Архітектура ПЗ", isIssued: true);

        var ex = Assert.Throws<DomainException>(() => copy.Issue());
        
        Assert.Contains("уже виданий", ex.Message);
        Assert.True(copy.IsIssued);
    }

    [Fact]
    // 2) Інваріант повернення: відмова повертати невиданий примірник
    public void Return_NonIssuedCopy_ThrowsDomainException()
    {
        var copy = new BookCopy("BC-102", "978-222", "Рефакторинг", isIssued: false);

        var ex = Assert.Throws<DomainException>(() => copy.Return());
        Assert.Contains("не виданий", ex.Message);
    }

    [Fact]
    // 3) Подія: підняття Changed рівно один раз при успішній видачі
    public void IssueCopy_RaisesChangedExactlyOnce()
    {
        var store = new InMemoryLoanStore();
        var service = new LendingService(store);
        var raisedEvents = new List<LoanChangedEventArgs>();

        service.Changed += (_, e) => raisedEvents.Add(e);

        var loan = service.IssueCopy("BC-200", "READER-99");

        Assert.Single(raisedEvents);
        Assert.Equal(LoanChangeKind.Issued, raisedEvents[0].Kind);
        Assert.Equal(loan.Id, raisedEvents[0].LoanId);
    }
}