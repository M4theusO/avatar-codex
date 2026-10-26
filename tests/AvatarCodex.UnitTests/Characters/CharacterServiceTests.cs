using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Application.Validators;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;

namespace AvatarCodex.UnitTests.Characters;

public class CharacterServiceTests
{
    private readonly FakeCharacterRepository _repository = new();
    private readonly CharacterService _service;

    public CharacterServiceTests()
    {
        _service = new CharacterService(_repository, new CharacterQueryParametersValidator());
    }

    [Fact]
    public async Task GetPaged_ConvertsQueryIntoFilter()
    {
        await _service.GetPagedAsync(new CharacterQueryParameters
        {
            Page = 2,
            PageSize = 5,
            Search = "  zuko  ",
            Type = "nonBender",
            Status = "deceased",
            Sort = "Element",
            Order = "DESC"
        });

        var filter = _repository.LastFilter!;
        Assert.Equal(2, filter.Page);
        Assert.Equal(5, filter.PageSize);
        Assert.Equal("zuko", filter.Search);
        Assert.Equal(CharacterType.NonBender, filter.Type);
        Assert.Equal(CharacterStatus.Deceased, filter.Status);
        Assert.Equal(CharacterSortField.Element, filter.Sort);
        Assert.True(filter.Descending);
    }

    [Fact]
    public async Task GetPaged_BlankSearchIsIgnored()
    {
        await _service.GetPagedAsync(new CharacterQueryParameters { Search = "   " });

        Assert.Null(_repository.LastFilter!.Search);
        Assert.Equal(CharacterSortField.Name, _repository.LastFilter.Sort);
        Assert.False(_repository.LastFilter.Descending);
    }

    [Fact]
    public async Task GetPaged_InvalidQuery_ThrowsValidationWithoutQueryingRepository()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetPagedAsync(new CharacterQueryParameters { PageSize = 51 }));

        Assert.Null(_repository.LastFilter);
    }

    [Fact]
    public async Task GetPaged_CalculatesTotalPages()
    {
        _repository.TotalItems = 37;

        var result = await _service.GetPagedAsync(new CharacterQueryParameters { PageSize = 12 });

        Assert.Equal(4, result.TotalPages);
    }

    [Fact]
    public async Task GetById_UnknownCharacter_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(42));
    }

    [Fact]
    public async Task GetAbilities_UnknownCharacter_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAbilitiesAsync(42));
    }

    private sealed class FakeCharacterRepository : ICharacterRepository
    {
        public CharacterFilter? LastFilter { get; private set; }
        public int TotalItems { get; set; }

        public Task<(IReadOnlyList<Character> Items, int TotalItems)> GetPagedAsync(
            CharacterFilter filter, CancellationToken cancellationToken)
        {
            LastFilter = filter;
            return Task.FromResult<(IReadOnlyList<Character>, int)>(([], TotalItems));
        }

        public Task<Character?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult<Character?>(null);

        public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) => Task.FromResult(false);

        public Task<IReadOnlyList<Ability>> GetAbilitiesAsync(int characterId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Ability>>([]);
    }
}
