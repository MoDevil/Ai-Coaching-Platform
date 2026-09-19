using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Programs.DTOs;
using AiCoachOs.Application.Programs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgramsController : ControllerBase
{
    private readonly IProgramService _programService;
    private readonly ICurrentCoachService _currentCoachService;

    public ProgramsController(IProgramService programService, ICurrentCoachService currentCoachService)
    {
        _programService = programService;
        _currentCoachService = currentCoachService;
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(ProgramDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Generate([FromBody] GenerateProgramRequestDto request, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var program = await _programService.GenerateProgramAsync(coachId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = program.Id }, program);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProgramDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var program = await _programService.GetProgramByIdAsync(coachId, id, cancellationToken);
        if (program == null)
        {
            return NotFound(new { message = $"Program with ID '{id}' was not found." });
        }

        return Ok(program);
    }

    [HttpGet("client/{clientId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ProgramSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByClientId(Guid clientId, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var programs = await _programService.GetProgramsByClientIdAsync(coachId, clientId, cancellationToken);
        return Ok(programs);
    }

    [HttpGet("client/{clientId:guid}/active")]
    [ProducesResponseType(typeof(ProgramDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveForClient(Guid clientId, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var program = await _programService.GetActiveProgramForClientAsync(coachId, clientId, cancellationToken);
        if (program == null)
        {
            return NotFound(new { message = $"No active program found for client '{clientId}'." });
        }

        return Ok(program);
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(ProgramDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateProgramStatusDto request, CancellationToken cancellationToken)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var updated = await _programService.UpdateProgramStatusAsync(coachId, id, request, cancellationToken);
        return Ok(updated);
    }
}
