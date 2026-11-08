using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.IntegrationTests.Api;

/// <summary>
/// O banco de teste é compartilhado: cada teste dá aos próprios usuários um XP muito maior que o dos outros
/// testes (milhões) e confere posições relativas entre eles.
/// </summary>
[Collection(ContentApiCollection.Name)]
public class RankingApiTests(ContentApiFixture api)
{
    private record TestUser(HttpClient Client, Guid Id, string Username);

    private async Task<TestUser> NewUserAsync(string prefix)
    {
        var client = api.CreateBrowserClient();
        var username = $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(username, $"{username}@example.com", "Senha123", "Senha123"));
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>(ContentApiFixture.JsonOptions))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return new TestUser(client, auth.User.Id, username);
    }

    /// <summary>Soma XP direto no banco, com a data escolhida (para testar o ranking do mês).</summary>
    private async Task GiveXpAsync(TestUser user, int amount, DateTimeOffset createdAt, string? element = null)
    {
        await using var db = api.Database.CreateContext();
        var entity = await db.Users.SingleAsync(u => u.Id == user.Id);
        entity.ExperiencePoints += amount;
        if (element is not null)
            entity.ElementId = (await db.Elements.SingleAsync(e => e.Name == element)).Id;
        db.ExperienceTransactions.Add(new ExperienceTransaction
        {
            UserId = user.Id,
            Amount = amount,
            Type = ExperienceType.ChallengeCompleted,
            SourceId = Random.Shared.Next(1_000_000, int.MaxValue),
            Description = "XP de teste do ranking",
            CreatedAt = createdAt
        });
        await db.SaveChangesAsync();
    }

    private static async Task<RankingDto> RankingAsync(TestUser viewer, string query = "")
    {
        var response = await viewer.Client.GetAsync($"/api/ranking{query}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RankingDto>(ContentApiFixture.JsonOptions))!;
    }

    [SkippableFact]
    public async Task AllTime_OrdersByXp_TiesShareThePosition_AndMarksTheViewer()
    {
        api.Database.SkipIfUnavailable();
        var now = DateTimeOffset.UtcNow;
        var first = await NewUserAsync("rank_a");
        var tied = await NewUserAsync("rank_b");
        var third = await NewUserAsync("rank_c");
        await GiveXpAsync(first, 50_000_000, now);
        await GiveXpAsync(tied, 50_000_000, now);
        await GiveXpAsync(third, 40_000_000, now);

        var response = await third.Client.GetAsync("/api/ranking?pageSize=50");
        var body = await response.Content.ReadAsStringAsync();
        var ranking = (await response.Content.ReadFromJsonAsync<RankingDto>(ContentApiFixture.JsonOptions))!;

        Assert.Equal(RankingRules.AllTime, ranking.Period);
        Assert.Null(ranking.PeriodStart);
        var mine = ranking.Entries.Items.Where(e => e.Username.StartsWith("rank_")).ToList();
        var a = mine.Single(e => e.Username == first.Username);
        var b = mine.Single(e => e.Username == tied.Username);
        var c = mine.Single(e => e.Username == third.Username);
        Assert.Equal(a.Position, b.Position);
        Assert.Equal(a.Position + 2, c.Position);
        Assert.Equal(50_000_010, a.Experience);
        Assert.Equal(LevelCalculator.GetLevel(50_000_010), a.Level);
        Assert.True(c.IsCurrentUser);
        Assert.False(a.IsCurrentUser);
        Assert.Equal(new RankingPositionDto(c.Position, 40_000_010), ranking.CurrentUser);

        // Só dados públicos do jogo.
        Assert.DoesNotContain("email", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(third.Id.ToString(), body);
    }

    [SkippableFact]
    public async Task Month_CountsOnlyThisMonthsXp()
    {
        api.Database.SkipIfUnavailable();
        var (monthStart, monthEnd) = RankingRules.MonthOf(DateTimeOffset.UtcNow);
        var veteran = await NewUserAsync("rank_old");
        await GiveXpAsync(veteran, 60_000_000, monthStart.AddDays(-1));

        var allTime = await RankingAsync(veteran);
        var month = await RankingAsync(veteran, "?period=month");

        Assert.Equal(60_000_010, allTime.CurrentUser!.Experience);
        Assert.Equal(RankingRules.Month, month.Period);
        Assert.Equal(monthStart, month.PeriodStart);
        Assert.Equal(monthEnd, month.PeriodEnd);
        // No mês, só o "Primeiro passo" do cadastro conta.
        Assert.Equal(10, month.CurrentUser!.Experience);
        Assert.DoesNotContain(month.Entries.Items, e => e.Experience >= 60_000_000);
    }

    [SkippableFact]
    public async Task Element_ListsOnlyThatElement_AndViewerWithoutElementIsOutside()
    {
        api.Database.SkipIfUnavailable();
        await using (var db = api.Database.CreateContext())
        {
            var fireId = (await db.Elements.SingleAsync(e => e.Name == "Fogo")).Id;
            var fireBender = await NewUserAsync("rank_fire");
            await GiveXpAsync(fireBender, 70_000_000, DateTimeOffset.UtcNow, element: "Fogo");
            var viewer = await NewUserAsync("rank_none");

            var ranking = await RankingAsync(viewer, $"?elementId={fireId}");

            Assert.Equal(fireId, ranking.ElementId);
            Assert.All(ranking.Entries.Items, e => Assert.Equal("Fogo", e.Element?.Name));
            Assert.Equal(fireBender.Username, ranking.Entries.Items[0].Username);
            Assert.Null(ranking.CurrentUser);
        }
    }

    [SkippableFact]
    public async Task RequiresLogin_AndValidatesTheQuery()
    {
        api.Database.SkipIfUnavailable();
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.CreateBrowserClient().GetAsync("/api/ranking")).StatusCode);
        var viewer = await NewUserAsync("rank_q");

        var response = await viewer.Client.GetAsync("/api/ranking?period=week&pageSize=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"field\":\"period\"", body);
        Assert.Contains("\"field\":\"pageSize\"", body);
    }
}
