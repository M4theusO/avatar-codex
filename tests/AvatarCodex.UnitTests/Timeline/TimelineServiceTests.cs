using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Application.Validators;
using AvatarCodex.Domain.Entities;
using FluentValidation;

namespace AvatarCodex.UnitTests.Timeline;

public class TimelineServiceTests
{
    /// <summary>Devolve os eventos como o banco faria sem tracking: cada um com a própria instância da era.</summary>
    private sealed class FakeTimelineRepository : ITimelineRepository
    {
        public List<TimelineEvent> Events { get; } = [];

        public Task<IReadOnlyList<(Era Era, int EventCount)>> GetErasAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<(Era, int)>>([]);

        public Task<IReadOnlyList<TimelineEvent>> GetEventsAsync(int? eraId, int? characterId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TimelineEvent>>([.. Events
                .Where(e => eraId is null || e.EraId == eraId)
                .Where(e => characterId is null || e.Characters.Any(c => c.Id == characterId))
                .OrderBy(e => e.Order)]);
    }

    private readonly FakeTimelineRepository _repository = new();
    private readonly TimelineService _service;

    public TimelineServiceTests()
    {
        _service = new TimelineService(_repository, new TimelineQueryParametersValidator());

        var aang = new Character { Id = 1, Name = "Aang", Description = "-", Biography = "-" };
        var zuko = new Character { Id = 2, Name = "Zuko", Description = "-", Biography = "-" };
        var korra = new Character { Id = 3, Name = "Korra", Description = "-", Biography = "-" };
        _repository.Events.AddRange(
        [
            Event(1, "Guerra", eraId: 2, eraOrder: 2, order: 2, zuko, aang),
            Event(2, "Wan", eraId: 1, eraOrder: 1, order: 1),
            Event(3, "Fim da guerra", eraId: 2, eraOrder: 2, order: 3, aang),
            Event(4, "Korra", eraId: 3, eraOrder: 3, order: 4, korra, aang)
        ]);
    }

    private static TimelineEvent Event(int id, string title, int eraId, int eraOrder, int order, params Character[] characters) => new()
    {
        Id = id,
        Title = title,
        Description = "-",
        DateLabel = "-",
        Order = order,
        EraId = eraId,
        Era = new Era { Id = eraId, Name = $"Era {eraId}", Description = "-", Order = eraOrder },
        Characters = [.. characters]
    };

    [Fact]
    public async Task GetTimeline_GroupsByEraInChronologicalOrder()
    {
        var timeline = await _service.GetTimelineAsync(new TimelineQueryParameters());

        Assert.Equal([1, 2, 3], timeline.Select(e => e.Id));
        Assert.Equal(["Guerra", "Fim da guerra"], timeline[1].Events.Select(e => e.Title));
        // Personagens por nome.
        Assert.Equal(["Aang", "Zuko"], timeline[1].Events[0].Characters.Select(c => c.Name));
    }

    [Fact]
    public async Task GetTimeline_ByCharacter_KeepsOnlyErasWithMatchingEvents()
    {
        var timeline = await _service.GetTimelineAsync(new TimelineQueryParameters { CharacterId = 1 });

        Assert.Equal([2, 3], timeline.Select(e => e.Id));
        Assert.Equal(3, timeline.Sum(e => e.Events.Count));
    }

    [Fact]
    public async Task GetTimeline_FilterWithoutResults_ReturnsEmpty()
    {
        var timeline = await _service.GetTimelineAsync(new TimelineQueryParameters { EraId = 1, CharacterId = 2 });

        Assert.Empty(timeline);
    }

    [Theory]
    [InlineData(0, null)]
    [InlineData(null, -1)]
    public async Task GetTimeline_InvalidIds_ThrowValidation(int? eraId, int? characterId)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetTimelineAsync(new TimelineQueryParameters { EraId = eraId, CharacterId = characterId }));
    }
}
