using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Photos.Dtos;
using AiCoachOs.Application.Photos.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/clients/{clientId:guid}/photos")]
public class ClientPhotosController : ControllerBase
{
    private readonly IPhotoVisionService _photoVisionService;
    private readonly ICurrentCoachService _currentCoachService;

    public ClientPhotosController(
        IPhotoVisionService photoVisionService,
        ICurrentCoachService currentCoachService)
    {
        _photoVisionService = photoVisionService;
        _currentCoachService = currentCoachService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientPhotoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientPhotoDto>> UploadPhoto(
        [FromRoute] Guid clientId,
        [FromBody] UploadPhotoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var photo = await _photoVisionService.UploadPhotoAsync(coachId, clientId, request, cancellationToken);
        return CreatedAtAction(nameof(GetPhotoObservation), new { clientId, photoId = photo.Id }, photo);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClientPhotoSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ClientPhotoSummaryDto>>> GetClientPhotos(
        [FromRoute] Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var photos = await _photoVisionService.GetClientPhotosAsync(coachId, clientId, cancellationToken);
        return Ok(photos);
    }

    [HttpGet("{photoId:guid}/url")]
    [ProducesResponseType(typeof(SignedPhotoUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SignedPhotoUrlDto>> GetSignedPhotoUrl(
        [FromRoute] Guid clientId,
        [FromRoute] Guid photoId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var signedUrl = await _photoVisionService.GetSignedPhotoUrlAsync(coachId, clientId, photoId, cancellationToken);
        return Ok(signedUrl);
    }

    [HttpDelete("{photoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeletePhoto(
        [FromRoute] Guid clientId,
        [FromRoute] Guid photoId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await _photoVisionService.DeletePhotoAsync(coachId, clientId, photoId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{photoId:guid}/analyze")]
    [ProducesResponseType(typeof(PhysiqueObservationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PhysiqueObservationResultDto>> AnalyzePhoto(
        [FromRoute] Guid clientId,
        [FromRoute] Guid photoId,
        [FromBody] AnalyzePhotoRequestDto? request = null,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var result = await _photoVisionService.AnalyzePhotoAsync(coachId, clientId, photoId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{photoId:guid}/observation")]
    [ProducesResponseType(typeof(PhysiqueObservationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PhysiqueObservationResultDto>> GetPhotoObservation(
        [FromRoute] Guid clientId,
        [FromRoute] Guid photoId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var observation = await _photoVisionService.GetPhotoObservationAsync(coachId, clientId, photoId, cancellationToken);
        if (observation == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Observation Not Found",
                Detail = $"No physique observation record is linked to photo {photoId}."
            });
        }

        return Ok(observation);
    }
}
