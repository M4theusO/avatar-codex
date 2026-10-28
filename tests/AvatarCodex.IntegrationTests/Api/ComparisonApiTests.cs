using System.Net;
using AvatarCodex.Application.DTOs;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ContentApiCollection.Name)]
public class ComparisonApiTests(ContentApiFixture api)
{
    private async Task<int> CharacterIdAsync(string name)
    {
        var page = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>($"/api/characters?search={name}");
        return page.Items.First(c => c.Name == name).Id;
    }

    [SkippableFact]
    public async Task Compare_IsPublicAndReturnsBothCharactersWithAttributes()
    {
        api.Database.SkipIfUnavailable();
        var zuko = await CharacterIdAsync("Zuko");
        var sokka = await CharacterIdAsync("Sokka");

        var result = await api.GetJsonAsync<CharacterComparisonDto>($"/api/characters/compare?ids={zuko},{sokka}");

        Assert.Equal(["Zuko", "Sokka"], result.Characters.Select(c => c.Name));
        var zukoInfo = result.Characters[0];
        Assert.Equal("Fogo", zukoInfo.Element?.Name);
        Assert.Equal("Nação do Fogo", zukoInfo.Nation?.Name);
        Assert.NotEmpty(zukoInfo.Abilities);
        Assert.NotEmpty(zukoInfo.Series);

        Assert.Equal(
            ["Domínio elemental", "Combate", "Agilidade", "Estratégia", "Liderança"],
            result.Attributes.Select(a => a.Name));
        Assert.All(result.Attributes, a =>
        {
            Assert.Equal(2, a.Values.Count);
            Assert.All(a.Values, v => Assert.InRange(v!.Value, 0, 100));
        });
        var bending = result.Attributes[0];
        Assert.Equal(0, bending.Values[1]);
        Assert.Equal(zuko, bending.LeaderId);
        Assert.Equal(sokka, result.Attributes.Single(a => a.Name == "Estratégia").LeaderId);
    }

    [SkippableFact]
    public async Task Compare_InvalidIdsAre400AndUnknownCharacterIs404()
    {
        api.Database.SkipIfUnavailable();
        var client = api.CreateClient();
        var aang = await CharacterIdAsync("Aang");

        var missing = await client.GetAsync("/api/characters/compare");
        var single = await client.GetAsync($"/api/characters/compare?ids={aang}");
        var same = await client.GetAsync($"/api/characters/compare?ids={aang},{aang}");
        var unknown = await client.GetAsync($"/api/characters/compare?ids={aang},999999");

        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, single.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, same.StatusCode);
        Assert.Contains("Escolha dois personagens diferentes.", await same.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }
}
