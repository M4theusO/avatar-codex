using System.Net;
using AvatarCodex.Application.DTOs;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ContentApiCollection.Name)]
public class CatalogApiTests(ContentApiFixture api)
{
    [SkippableFact]
    public async Task Nations_ListsAllWithCharacterCountAndElementOrder()
    {
        api.Database.SkipIfUnavailable();

        var nations = await api.GetJsonAsync<List<NationDto>>("/api/nations");

        Assert.Equal(
            ["Tribos da Água", "Reino da Terra", "Nação do Fogo", "Nômades do Ar", "República Unida"],
            nations.Select(n => n.Name));
        Assert.Null(nations.Last().Element);
        Assert.All(nations, n => Assert.True(n.CharacterCount > 0));
    }

    [SkippableFact]
    public async Task NationDetails_IncludesCharactersWithTheirNation()
    {
        api.Database.SkipIfUnavailable();
        var nations = await api.GetJsonAsync<List<NationDto>>("/api/nations");
        var fireNation = nations.Single(n => n.Name == "Nação do Fogo");

        var details = await api.GetJsonAsync<NationDetailsDto>($"/api/nations/{fireNation.Id}");

        Assert.Equal(fireNation.CharacterCount, details.Characters.Count);
        Assert.Contains(details.Characters, c => c.Name == "Zuko");
        Assert.All(details.Characters, c => Assert.Equal("Nação do Fogo", c.Nation?.Name));
    }

    [SkippableFact]
    public async Task NationDetails_UnknownNation_ReturnsNotFound()
    {
        api.Database.SkipIfUnavailable();

        var response = await api.CreateClient().GetAsync("/api/nations/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [SkippableFact]
    public async Task Elements_ListsFourElementsWithSubtypes()
    {
        api.Database.SkipIfUnavailable();

        var elements = await api.GetJsonAsync<List<ElementDto>>("/api/elements");

        Assert.Equal(["Água", "Terra", "Fogo", "Ar"], elements.Select(e => e.Name));
        Assert.Contains(elements.Single(e => e.Name == "Terra").BendingSubtypes, b => b.Name == "Dobra de metal");
    }

    [SkippableFact]
    public async Task Abilities_FiltersByElement()
    {
        api.Database.SkipIfUnavailable();
        var elements = await api.GetJsonAsync<List<ElementDto>>("/api/elements");
        var air = elements.Single(e => e.Name == "Ar");

        var abilities = await api.GetJsonAsync<List<AbilityDto>>($"/api/abilities?elementId={air.Id}");

        Assert.NotEmpty(abilities);
        Assert.All(abilities, a => Assert.Equal("Ar", a.Element?.Name));
    }

    [SkippableFact]
    public async Task Series_ListsBothSeries()
    {
        api.Database.SkipIfUnavailable();

        var series = await api.GetJsonAsync<List<SeriesDto>>("/api/series");

        Assert.Equal(["ATLA", "LOK"], series.Select(s => s.Code));
    }
}
