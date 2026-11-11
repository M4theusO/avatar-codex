using AvatarCodex.Api.Extensions;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

/// <summary>Dados do usuário autenticado. O id vem sempre do token, nunca do cliente.</summary>
[ApiController]
[Authorize]
[Route("api/users/me")]
[Produces("application/json")]
[ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
public class UsersController(
    UserService users,
    FavoriteService favorites,
    DiscoveryService discoveries,
    AchievementService achievements,
    QuizService quizzes,
    DashboardService dashboard,
    DailyChallengeService challenges,
    AccountService account,
    AvatarService avatars) : ControllerBase
{
    /// <summary>
    /// Edita o perfil (nome de usuário). Devolve o usuário atualizado. Nome em uso → 409;
    /// conta de demonstração → 422.
    /// </summary>
    [HttpPatch]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UserDto>> Update(UpdateProfileRequest request, CancellationToken cancellationToken) =>
        Ok(await account.UpdateProfileAsync(User.GetUserId(), request, cancellationToken));

    /// <summary>
    /// Torna o perfil público (para quem está logado) ou privado. Devolve o usuário atualizado.
    /// Conta de demonstração → 422 (fica sempre visível).
    /// </summary>
    [HttpPut("privacy")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UserDto>> UpdatePrivacy(UpdatePrivacyRequest request, CancellationToken cancellationToken) =>
        Ok(await account.UpdatePrivacyAsync(User.GetUserId(), request, cancellationToken));

    /// <summary>
    /// Envia a foto de perfil (multipart, campo "file"; JPEG, PNG ou WebP até 512 KB). Substitui a anterior:
    /// cada usuário tem uma só. Devolve o usuário com o novo avatarUrl.
    /// </summary>
    [HttpPut("avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(AvatarRules.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = AvatarRules.MaxRequestBytes)]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UserDto>> UploadAvatar(IFormFile? file, CancellationToken cancellationToken) =>
        Ok(await avatars.UploadAsync(User.GetUserId(), await file.ReadBytesAsync(cancellationToken), cancellationToken));

    /// <summary>Remove a foto de perfil (volta para as iniciais).</summary>
    [HttpDelete("avatar")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UserDto>> RemoveAvatar(CancellationToken cancellationToken) =>
        Ok(await avatars.RemoveAsync(User.GetUserId(), cancellationToken));

    /// <summary>
    /// Exclui a conta e todos os dados dela, confirmando a senha. Senha errada → 400 (campo password);
    /// conta de demonstração → 422. Apaga também o cookie de refresh.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(DeleteAccountRequest request, CancellationToken cancellationToken)
    {
        await account.DeleteAsync(User.GetUserId(), request, cancellationToken);
        Response.ClearRefreshCookie();
        return NoContent();
    }

    /// <summary>
    /// Página inicial do usuário numa chamada: progresso, estatísticas, desafios do dia, conquistas,
    /// descobertas, recomendações, quiz em destaque e favoritos.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType<DashboardDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken cancellationToken) =>
        Ok(await dashboard.GetAsync(User.GetUserId(), cancellationToken));

    /// <summary>Descobertas, favoritos, conquistas, quizzes e taxa de acerto.</summary>
    [HttpGet("statistics")]
    [ProducesResponseType<UserStatisticsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserStatisticsDto>> GetStatistics(CancellationToken cancellationToken) =>
        Ok(await users.GetStatisticsAsync(User.GetUserId(), cancellationToken));

    /// <summary>Favoritos, mais recentes primeiro.</summary>
    [HttpGet("favorites")]
    [ProducesResponseType<PagedResult<CharacterListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CharacterListItemDto>>> GetFavorites(
        [FromQuery] PageQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await favorites.GetPagedAsync(User.GetUserId(), query, cancellationToken));

    /// <summary>Histórico de XP (cada movimentação), mais recente primeiro.</summary>
    [HttpGet("experience")]
    [ProducesResponseType<PagedResult<ExperienceTransactionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ExperienceTransactionDto>>> GetExperience(
        [FromQuery] PageQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await users.GetExperienceHistoryAsync(User.GetUserId(), query, cancellationToken));

    /// <summary>Tentativas de quiz (todas, inclusive as sem XP), mais recentes primeiro.</summary>
    [HttpGet("quiz-history")]
    [ProducesResponseType<PagedResult<QuizAttemptDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<QuizAttemptDto>>> GetQuizHistory(
        [FromQuery] PageQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await quizzes.GetHistoryAsync(User.GetUserId(), query, cancellationToken));

    /// <summary>Catálogo completo: desbloqueadas com data, bloqueadas com progresso.</summary>
    [HttpGet("achievements")]
    [ProducesResponseType<IReadOnlyList<UserAchievementDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserAchievementDto>>> GetAchievements(CancellationToken cancellationToken) =>
        Ok(await achievements.GetForUserAsync(User.GetUserId(), cancellationToken));

    /// <summary>Desafios diários concluídos, mais recentes primeiro.</summary>
    [HttpGet("challenges")]
    [ProducesResponseType<PagedResult<CompletedChallengeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CompletedChallengeDto>>> GetChallenges(
        [FromQuery] PageQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await challenges.GetHistoryAsync(User.GetUserId(), query, cancellationToken));

    /// <summary>Descobertas de personagens e nações, mais recentes primeiro.</summary>
    [HttpGet("discoveries")]
    [ProducesResponseType<PagedResult<DiscoveryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<DiscoveryDto>>> GetDiscoveries(
        [FromQuery] PageQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await discoveries.GetHistoryAsync(User.GetUserId(), query, cancellationToken));
}
