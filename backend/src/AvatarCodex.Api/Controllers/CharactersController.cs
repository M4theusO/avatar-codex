using AvatarCodex.Api.Extensions;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

[ApiController]
[Route("api/characters")]
[Produces("application/json")]
public class CharactersController(
    CharacterService characters,
    DiscoveryService discoveries,
    FavoriteService favorites) : ControllerBase
{
    /// <summary>Lista paginada com busca, filtros e ordenação. Com token, preenche isFavorite/isDiscovered.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<CharacterListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CharacterListItemDto>>> GetAll(
        [FromQuery] CharacterQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await characters.GetPagedAsync(query, User.GetUserIdOrNull(), cancellationToken));

    /// <summary>Detalhe com nação, elemento, subtipos, habilidades, séries e relações.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<CharacterDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CharacterDetailsDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await characters.GetByIdAsync(id, User.GetUserIdOrNull(), cancellationToken));

    [HttpGet("{id:int}/abilities")]
    [ProducesResponseType<IReadOnlyList<AbilityDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AbilityDto>>> GetAbilities(int id, CancellationToken cancellationToken) =>
        Ok(await characters.GetAbilitiesAsync(id, cancellationToken));

    /// <summary>Registra a descoberta (idempotente): +10 XP só na primeira vez.</summary>
    [Authorize]
    [HttpPost("{id:int}/discovery")]
    [ProducesResponseType<DiscoveryResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiscoveryResultDto>> Discover(int id, CancellationToken cancellationToken) =>
        Ok(await discoveries.DiscoverCharacterAsync(User.GetUserId(), id, cancellationToken));

    [Authorize]
    [HttpPost("{id:int}/favorite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFavorite(int id, CancellationToken cancellationToken)
    {
        await favorites.AddAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:int}/favorite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFavorite(int id, CancellationToken cancellationToken)
    {
        await favorites.RemoveAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }
}
