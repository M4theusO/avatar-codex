using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Entities;

namespace AvatarCodex.UnitTests.ElementTest;

public class ElementTestRulesTests
{
    private const int Water = 1, Earth = 2, Fire = 3, Air = 4;
    private static readonly int[] Elements = [Water, Earth, Fire, Air];

    /// <summary>Pergunta n com uma opção por elemento; o id da opção é n * 10 + elemento.</summary>
    private static ElementTestQuestion Question(int order) => new()
    {
        Id = order,
        Order = order,
        Text = $"Pergunta {order}",
        Options = [.. Elements.Select(e => new ElementTestOption { Id = order * 10 + e, QuestionId = order, ElementId = e, Text = "", Order = e })]
    };

    private static (int WinnerId, IReadOnlyList<ElementScore> Scores) Answer(params int[] elementPerQuestion)
    {
        var questions = elementPerQuestion.Select((_, i) => Question(i + 1)).ToList();
        var selected = elementPerQuestion.Select((element, i) => (Question: i + 1, Option: (i + 1) * 10 + element))
            .ToDictionary(a => a.Question, a => a.Option);
        return ElementTestRules.Score(questions, selected, Elements);
    }

    [Fact]
    public void Score_ElementWithMostAnswersWins_AndEveryElementIsListed()
    {
        var (winner, scores) = Answer(Fire, Water, Fire, Air, Fire);

        Assert.Equal(Fire, winner);
        Assert.Equal(
            [new ElementScore(Water, 1), new ElementScore(Earth, 0), new ElementScore(Fire, 3), new ElementScore(Air, 1)],
            scores);
    }

    [Fact]
    public void Score_Tie_IsWonByTheElementChosenLastAmongTheTied()
    {
        // Água e Terra empatam com 2; a última escolha entre elas foi Água (a Fogo da última pergunta não está empatada).
        Assert.Equal(Water, Answer(Earth, Water, Earth, Water, Fire).WinnerId);
        Assert.Equal(Earth, Answer(Water, Earth, Water, Earth).WinnerId);
    }

    [Fact]
    public void Score_SameAnswersAlwaysGiveTheSameElement()
    {
        var first = Answer(Air, Earth, Air, Earth, Water, Fire);
        var second = Answer(Air, Earth, Air, Earth, Water, Fire);

        Assert.Equal(first.WinnerId, second.WinnerId);
        Assert.Equal(Earth, first.WinnerId);
    }
}
