using AvatarCodex.Api.Extensions;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

[ApiController]
[Route("api/quizzes")]
[Produces("application/json")]
public class QuizzesController(QuizService quizzes) : ControllerBase
{
    /// <summary>Catálogo (filtros category e difficulty). Com token, inclui o melhor resultado do usuário.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<QuizListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<QuizListItemDto>>> GetAll(
        [FromQuery] QuizQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await quizzes.GetAllAsync(query, User.GetUserIdOrNull(), cancellationToken));

    /// <summary>Perguntas e opções para jogar, sem as respostas corretas.</summary>
    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType<QuizDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await quizzes.GetForPlayAsync(id, cancellationToken));

    /// <summary>Submete as respostas: correção, tentativa registrada e XP (só nas primeiras vezes).</summary>
    [Authorize]
    [HttpPost("{id:int}/attempts")]
    [ProducesResponseType<QuizAttemptResultDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizAttemptResultDto>> Submit(
        int id, SubmitQuizAttemptRequest request, CancellationToken cancellationToken)
    {
        var result = await quizzes.SubmitAsync(User.GetUserId(), id, request, cancellationToken);
        // A tentativa aparece em /api/users/me/quiz-history; não há GET próprio por id.
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
