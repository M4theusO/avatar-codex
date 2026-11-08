using AvatarCodex.Api.Extensions;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

/// <summary>Teste de elemento (RF-ELTEST-01): o resultado vai para o perfil do usuário.</summary>
[ApiController]
[Authorize]
[Route("api/element-test")]
[Produces("application/json")]
[ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
public class ElementTestController(ElementTestService elementTest) : ControllerBase
{
    /// <summary>Perguntas e opções, sem o elemento de cada opção.</summary>
    [HttpGet]
    [ProducesResponseType<ElementTestDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ElementTestDto>> Get(CancellationToken cancellationToken) =>
        Ok(await elementTest.GetAsync(cancellationToken));

    /// <summary>
    /// Respostas → elemento calculado no backend e salvo no perfil (refazer substitui). +30 XP só na primeira vez.
    /// </summary>
    [HttpPost("submissions")]
    [ProducesResponseType<ElementTestResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ElementTestResultDto>> Submit(
        SubmitElementTestRequest request, CancellationToken cancellationToken) =>
        Ok(await elementTest.SubmitAsync(User.GetUserId(), request, cancellationToken));
}
