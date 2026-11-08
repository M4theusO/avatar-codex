using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using AvatarCodex.Application.Validators;

namespace AvatarCodex.UnitTests.Ranking;

public class RankingRulesTests
{
    [Fact]
    public void Positions_TiesShareThePositionAndTheNextOneSkips()
    {
        Assert.Equal([1, 2, 2, 4, 5, 5, 5, 8], RankingRules.Positions(1, 0, [90, 70, 70, 50, 40, 40, 40, 10]));
    }

    [Fact]
    public void Positions_SecondPage_ContinuesFromTheGlobalOrder()
    {
        // Página 2 de tamanho 3: o primeiro item empata com o último da página anterior (posição 2).
        Assert.Equal([2, 5, 5], RankingRules.Positions(firstPosition: 2, skip: 3, [70, 50, 50]));
        Assert.Empty(RankingRules.Positions(0, 0, []));
    }

    [Theory]
    [InlineData("2026-11-08T15:30:00Z", "2026-11-01T00:00:00Z", "2026-12-01T00:00:00Z")]
    [InlineData("2026-12-31T23:59:59Z", "2026-12-01T00:00:00Z", "2027-01-01T00:00:00Z")]
    // 21h de 31/10 em Brasília já é 1º de novembro em UTC.
    [InlineData("2026-10-31T21:00:00-03:00", "2026-11-01T00:00:00Z", "2026-12-01T00:00:00Z")]
    public void MonthOf_IsTheCurrentUtcMonth(string now, string start, string end)
    {
        var (monthStart, monthEnd) = RankingRules.MonthOf(DateTimeOffset.Parse(now));

        Assert.Equal(DateTimeOffset.Parse(start), monthStart);
        Assert.Equal(DateTimeOffset.Parse(end), monthEnd);
    }

    [Theory]
    [InlineData("all", true, false)]
    [InlineData("MONTH", true, true)]
    [InlineData("week", false, false)]
    [InlineData(null, false, false)]
    public void TryParsePeriod_AcceptsAllAndMonth(string? period, bool valid, bool monthly)
    {
        Assert.Equal(valid, RankingRules.TryParsePeriod(period, out var isMonthly));
        Assert.Equal(monthly, isMonthly);
    }

    [Fact]
    public void Validator_RejectsInvalidPeriodElementAndPage()
    {
        var result = new RankingQueryParametersValidator().Validate(
            new RankingQueryParameters { Period = "week", ElementId = 0, Page = 0, PageSize = 51 });

        Assert.Equal(
            ["Period", "ElementId", "Page", "PageSize"],
            result.Errors.Select(e => e.PropertyName));
    }
}
