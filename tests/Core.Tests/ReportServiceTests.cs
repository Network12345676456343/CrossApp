using System.Linq;
using Xunit;

namespace Core.Tests;

public class ReportServiceTests
{
    [Theory]
    [InlineData("READER-01", 2)]
    [InlineData("READER-02", 1)]
    [InlineData("NON_EXISTING", 0)]
    // [Theory] з [InlineData]: перевірка вибірки за читачами
    public void LoansByReader_CountsAccurately(string readerId, int expectedCount)
    {
        var loans = TestData.Loans();
        var count = loans.Count(l => l.ReaderId == readerId);
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    // Детермінований топ читачів
    public void TopReaders_IsDeterministic()
    {
        var loans = TestData.Loans();
        
        var topReader = loans
            .GroupBy(l => l.ReaderId)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => g.Key)
            .First();

        Assert.Equal("READER-01", topReader);
    }
}