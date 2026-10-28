using System.Net;
using AvatarCodex.Application.DTOs;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ContentApiCollection.Name)]
public class TimelineApiTests(ContentApiFixture api)
{
    private async Task<int> CharacterIdAsync(string name)
    {
        var page = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>($"/api/characters?search={name}");
        return page.Items.First(c => c.Name == name).Id;
    }

    [SkippableFact]
    public async Task Eras_AreChronologicalWithEventCounts()
    {
        api.Database.SkipIfUnavailable();

        var eras = await api.GetJsonAsync<List<EraDto>>("/api/eras");

        Assert.Equal(
            ["A Era de Wan", "Os Avatares do Passado", "A Guerra dos Cem Anos", "A Era de Korra"],
            eras.Select(e => e.Name));
        Assert.Equal(17, eras.Sum(e => e.EventCount));
        Assert.All(eras, e => Assert.True(e.EventCount > 0));
    }

    [SkippableFact]
    public async Task Timeline_IsPublicAndGroupedByEraInChronologicalOrder()
    {
        api.Database.SkipIfUnavailable();

        var timeline = await api.GetJsonAsync<List<TimelineEraDto>>("/api/timeline");

        Assert.Equal(4, timeline.Count);
        Assert.Equal("Wan se torna o primeiro Avatar", timeline[0].Events[0].Title);
        Assert.Equal("O Império da Terra", timeline[^1].Events[^1].Title);
        var war = timeline.Single(e => e.Name == "A Guerra dos Cem Anos");
        Assert.Equal("O genocídio dos Nômades do Ar", war.Events[0].Title);
        Assert.Equal("0 AG", war.Events[0].DateLabel);
        var siege = war.Events.Single(e => e.Title == "O Cerco do Norte");
        Assert.Equal(siege.Characters.Select(c => c.Name).Order(), siege.Characters.Select(c => c.Name));
        Assert.Contains(siege.Characters, c => c.Name == "Zuko" && c.Element is not null);
    }

    [SkippableFact]
    public async Task Timeline_FiltersByCharacterAndEra()
    {
        api.Database.SkipIfUnavailable();
        var zuko = await CharacterIdAsync("Zuko");
        var eras = await api.GetJsonAsync<List<EraDto>>("/api/eras");
        var korraEra = eras.Single(e => e.Name == "A Era de Korra").Id;

        var byZuko = await api.GetJsonAsync<List<TimelineEraDto>>($"/api/timeline?characterId={zuko}");
        var byEra = await api.GetJsonAsync<List<TimelineEraDto>>($"/api/timeline?eraId={korraEra}");
        var both = await api.GetJsonAsync<List<TimelineEraDto>>($"/api/timeline?eraId={korraEra}&characterId={zuko}");

        Assert.Equal(["A Guerra dos Cem Anos", "A Era de Korra"], byZuko.Select(e => e.Name));
        Assert.All(byZuko.SelectMany(e => e.Events), e => Assert.Contains(e.Characters, c => c.Id == zuko));
        Assert.Equal(["A Era de Korra"], byEra.Select(e => e.Name));
        Assert.Equal(["Nasce a Cidade República"], both.Single().Events.Select(e => e.Title));
    }

    [SkippableFact]
    public async Task Timeline_UnknownFilterIsEmptyAndInvalidFilterIs400()
    {
        api.Database.SkipIfUnavailable();
        var client = api.CreateClient();

        var unknown = await api.GetJsonAsync<List<TimelineEraDto>>("/api/timeline?characterId=999999");
        var invalid = await client.GetAsync("/api/timeline?eraId=0");
        var notANumber = await client.GetAsync("/api/timeline?characterId=zuko");

        Assert.Empty(unknown);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, notANumber.StatusCode);
    }
}
