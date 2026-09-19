using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Clients.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClientSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ClientSummaryDto>>> GetClients(CancellationToken cancellationToken)
    {
        var clients = await _clientService.GetCoachClientsAsync(cancellationToken);
        return Ok(clients);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientDto>> GetClientById(Guid id, CancellationToken cancellationToken)
    {
        var client = await _clientService.GetClientByIdAsync(id, cancellationToken);
        return Ok(client);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientDto>> CreateClient([FromBody] CreateClientRequestDto request, CancellationToken cancellationToken)
    {
        var client = await _clientService.CreateClientAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetClientById), new { id = client.Id }, client);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientDto>> UpdateClient(Guid id, [FromBody] UpdateClientRequestDto request, CancellationToken cancellationToken)
    {
        var client = await _clientService.UpdateClientAsync(id, request, cancellationToken);
        return Ok(client);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ArchiveClient(Guid id, CancellationToken cancellationToken)
    {
        await _clientService.ArchiveClientAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/consent")]
    [ProducesResponseType(typeof(ConsentRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ConsentRecordDto>> RecordConsent(
        Guid id,
        [FromBody] RecordConsentRequest request,
        CancellationToken cancellationToken)
    {
        var consent = await _clientService.RecordConsentAsync(id, request.ConsentType, request.IsGranted, request.Notes, cancellationToken);
        return Ok(consent);
    }

    [HttpGet("{id:guid}/consent")]
    [ProducesResponseType(typeof(IReadOnlyList<ConsentRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ConsentRecordDto>>> GetConsents(Guid id, CancellationToken cancellationToken)
    {
        var consents = await _clientService.GetClientConsentsAsync(id, cancellationToken);
        return Ok(consents);
    }
}

public record RecordConsentRequest(
    string ConsentType,
    bool IsGranted,
    string? Notes = null
);
