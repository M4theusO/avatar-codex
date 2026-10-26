using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

[ApiController]
[Route("api/characters")]
[Produces("application/json")]
public class CharactersController(CharacterService characters) : ControllerBase
{
    /// <summary>Lista paginada com busca, filtros e ordenação.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<CharacterListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CharacterListItemDto>>> GetAll(
        [FromQuery] CharacterQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await characters.GetPagedAsync(query, cancellationToken));

    /// <summary>Detalhe com nação, elemento, subtipos, habilidades, séries e relações.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<CharacterDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CharacterDetailsDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await characters.GetByIdAsync(id, cancellationToken));

    [HttpGet("{id:int}/abilities")]
    [ProducesResponseType<IReadOnlyList<AbilityDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AbilityDto>>> GetAbilities(int id, CancellationToken cancellationToken) =>
        Ok(await characters.GetAbilitiesAsync(id, cancellationToken));
}
