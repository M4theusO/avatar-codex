using System.Net;
using System.Net.Http.Json;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Domain.Enums;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ContentApiCollection.Name)]
public class CharactersApiTests(ContentApiFixture api)
{
    private const int SeededCharacters = 37;

    [SkippableFact]
    public async Task List_DefaultsToFirstPageOrderedByName()
    {
        api.Database.SkipIfUnavailable();

        var result = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>("/api/characters");

        Assert.Equal(1, result.Page);
        Assert.Equal(12, result.PageSize);
        Assert.Equal(SeededCharacters, result.TotalItems);
        Assert.Equal(4, result.TotalPages);
        Assert.Equal(12, result.Items.Count);
        Assert.Equal("Aang", result.Items[0].Name);
    }

    [SkippableFact]
    public async Task List_LastPageHasRemainingItems()
    {
        api.Database.SkipIfUnavailable();

        var result = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>("/api/characters?page=4&pageSize=12");

        Assert.Single(result.Items);
    }

    [SkippableFact]
    public async Task List_PagesDoNotRepeatCharacters()
    {
        api.Database.SkipIfUnavailable();

        var names = new List<string>();
        for (var page = 1; page <= 4; page++)
        {
            var result = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>($"/api/characters?page={page}&sort=nation");
            names.AddRange(result.Items.Select(c => c.Name));
        }

        Assert.Equal(SeededCharacters, names.Distinct().Count());
    }

    [SkippableTheory]
    [InlineData("zuko", "Zuko")]
    [InlineData("ZUKO", "Zuko")]
    [InlineData("rei fênix", "Ozai")]
    public async Task List_SearchesByNameAndAliasIgnoringCase(string search, string expected)
    {
        api.Database.SkipIfUnavailable();

        var result = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>(
            $"/api/characters?search={Uri.EscapeDataString(search)}");

        Assert.Contains(result.Items, c => c.Name == expected);
    }

    [SkippableFact]
    public async Task List_SearchTreatsWildcardsLiterally()
    {
        api.Database.SkipIfUnavailable();

        var result = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>("/api/characters?search=%25");

        Assert.Equal(0, result.TotalItems);
    }

    [SkippableFact]
    public async Task List_FiltersByType()
    {
        api.Database.SkipIfUnavailable();

        var result = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>("/api/characters?type=avatar");

        Assert.Equal(["Aang", "Korra", "Kyoshi", "Roku", "Wan"], result.Items.Select(c => c.Name));
    }

    [SkippableFact]
    public async Task List_CombinesFilters()
    {
        api.Database.SkipIfUnavailable();
        var elements = await api.GetJsonAsync<List<ElementDto>>("/api/elements");
        var fire = elements.Single(e => e.Name == "Fogo");

        var result = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>(
            $"/api/characters?elementId={fire.Id}&status=deceased&pageSize=50");

        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, c =>
        {
            Assert.Equal("Fogo", c.Element?.Name);
            Assert.Equal(CharacterStatus.Deceased, c.Status);
        });
    }

    [SkippableFact]
    public async Task List_FiltersBySeries()
    {
        api.Database.SkipIfUnavailable();
        var series = await api.GetJsonAsync<List<SeriesDto>>("/api/series");
        var lok = series.Single(s => s.Code == "LOK");

        var result = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>($"/api/characters?seriesId={lok.Id}&pageSize=50");

        Assert.Contains(result.Items, c => c.Name == "Korra");
        Assert.Contains(result.Items, c => c.Name == "Zuko");
        Assert.DoesNotContain(result.Items, c => c.Name == "Azula");
    }

    [SkippableFact]
    public async Task List_SortsByNameDescending()
    {
        api.Database.SkipIfUnavailable();

        var result = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>("/api/characters?sort=name&order=desc&pageSize=2");

        Assert.Equal(["Zuko", "Zhao"], result.Items.Select(c => c.Name));
    }

    [SkippableFact]
    public async Task List_SortByElementKeepsCharactersWithoutElementLast()
    {
        api.Database.SkipIfUnavailable();

        var result = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>("/api/characters?sort=element&order=desc&pageSize=50");

        var firstWithoutElement = result.Items.ToList().FindIndex(c => c.Element is null);
        Assert.True(firstWithoutElement > 0);
        Assert.All(result.Items.Skip(firstWithoutElement), c => Assert.Null(c.Element));
    }

    [SkippableTheory]
    [InlineData("pageSize=100", "pageSize")]
    [InlineData("page=0", "page")]
    [InlineData("type=dragon", "type")]
    [InlineData("status=alive2", "status")]
    [InlineData("sort=age", "sort")]
    [InlineData("order=up", "order")]
    [InlineData("page=abc", "page")]
    public async Task List_InvalidParameters_ReturnBadRequestWithFieldErrors(string query, string field)
    {
        api.Database.SkipIfUnavailable();

        var response = await api.CreateClient().GetAsync($"/api/characters?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ContentApiFixture.JsonOptions);
        Assert.Equal(400, error!.Status);
        Assert.Contains(error.Errors, e => e.Field == field);
    }

    [SkippableFact]
    public async Task GetById_ReturnsFullDetails()
    {
        api.Database.SkipIfUnavailable();
        var zukoId = await FindIdAsync("Zuko");

        var zuko = await api.GetJsonAsync<CharacterDetailsDto>($"/api/characters/{zukoId}");

        Assert.Equal("Nação do Fogo", zuko.Nation?.Name);
        Assert.Equal("Fogo", zuko.Element?.Name);
        Assert.Contains("Príncipe Zuko", zuko.Aliases);
        Assert.Contains(zuko.Abilities, a => a.Name == "Redirecionamento de relâmpago");
        Assert.Contains(zuko.Series, s => s.Code == "LOK");
        Assert.Contains(zuko.Relationships, r =>
            r.Character.Name == "Iroh" && r.Type == RelationshipType.Family && r.Description == "Tio");
    }

    [SkippableFact]
    public async Task GetById_UnknownCharacter_ReturnsNotFound()
    {
        api.Database.SkipIfUnavailable();

        var response = await api.CreateClient().GetAsync("/api/characters/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(ContentApiFixture.JsonOptions);
        Assert.Equal(404, error!.Status);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
    }

    [SkippableFact]
    public async Task GetAbilities_ReturnsCharacterAbilities()
    {
        api.Database.SkipIfUnavailable();
        var tophId = await FindIdAsync("Toph Beifong");

        var abilities = await api.GetJsonAsync<List<AbilityDto>>($"/api/characters/{tophId}/abilities");

        Assert.Contains(abilities, a => a.Name == "Sentido sísmico");
    }

    [SkippableFact]
    public async Task GetAbilities_UnknownCharacter_ReturnsNotFound()
    {
        api.Database.SkipIfUnavailable();

        var response = await api.CreateClient().GetAsync("/api/characters/999999/abilities");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<int> FindIdAsync(string name)
    {
        var result = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>(
            $"/api/characters?search={Uri.EscapeDataString(name)}");
        return result.Items.Single(c => c.Name == name).Id;
    }
}
