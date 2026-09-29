using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Videos.Dtos;
using AiCoachOs.Application.Videos.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCoachOs.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/clients/{clientId:guid}/videos")]
public class ClientVideosController : ControllerBase
{
    private readonly IVideoAnalysisService _videoAnalysisService;
    private readonly ICurrentCoachService _currentCoachService;

    public ClientVideosController(
        IVideoAnalysisService videoAnalysisService,
        ICurrentCoachService currentCoachService)
    {
        _videoAnalysisService = videoAnalysisService;
        _currentCoachService = currentCoachService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientVideoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientVideoDto>> UploadVideo(
        [FromRoute] Guid clientId,
        [FromBody] UploadVideoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var video = await _videoAnalysisService.UploadVideoAsync(coachId, clientId, request, cancellationToken);
        return CreatedAtAction(nameof(GetVideoObservation), new { clientId, videoId = video.Id }, video);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClientVideoSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ClientVideoSummaryDto>>> GetClientVideos(
        [FromRoute] Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var videos = await _videoAnalysisService.GetClientVideosAsync(coachId, clientId, cancellationToken);
        return Ok(videos);
    }

    [HttpGet("{videoId:guid}/url")]
    [ProducesResponseType(typeof(SignedMediaUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SignedMediaUrlDto>> GetSignedVideoUrl(
        [FromRoute] Guid clientId,
        [FromRoute] Guid videoId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var signedUrl = await _videoAnalysisService.GetSignedVideoUrlAsync(coachId, clientId, videoId, cancellationToken);
        return Ok(signedUrl);
    }

    [HttpGet("{videoId:guid}/frames")]
    [ProducesResponseType(typeof(IReadOnlyList<VideoFrameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<VideoFrameDto>>> GetVideoFrames(
        [FromRoute] Guid clientId,
        [FromRoute] Guid videoId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var frames = await _videoAnalysisService.GetVideoFramesAsync(coachId, clientId, videoId, cancellationToken);
        return Ok(frames);
    }

    [HttpDelete("{videoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteVideo(
        [FromRoute] Guid clientId,
        [FromRoute] Guid videoId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await _videoAnalysisService.AnonymizeVideoAsync(coachId, clientId, videoId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{videoId:guid}/analyze")]
    [ProducesResponseType(typeof(EnqueueVideoAnalysisResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EnqueueVideoAnalysisResponseDto>> AnalyzeVideo(
        [FromRoute] Guid clientId,
        [FromRoute] Guid videoId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);

        try
        {
            var response = await _videoAnalysisService.EnqueueVideoAnalysisAsync(coachId, clientId, videoId, cancellationToken);
            return Accepted(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpGet("{videoId:guid}/analysis-status")]
    [ProducesResponseType(typeof(VideoAnalysisJobStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<VideoAnalysisJobStatusDto>> GetAnalysisStatus(
        [FromRoute] Guid clientId,
        [FromRoute] Guid videoId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var status = await _videoAnalysisService.GetAnalysisStatusAsync(coachId, clientId, videoId, cancellationToken);
        return Ok(status);
    }

    [HttpGet("{videoId:guid}/observation")]
    [ProducesResponseType(typeof(ExerciseTechniqueObservationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ExerciseTechniqueObservationResultDto>> GetVideoObservation(
        [FromRoute] Guid clientId,
        [FromRoute] Guid videoId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var observation = await _videoAnalysisService.GetVideoObservationAsync(coachId, clientId, videoId, cancellationToken);
        if (observation == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Observation Not Found",
                Detail = $"No exercise technique observation record is linked to video {videoId}."
            });
        }

        return Ok(observation);
    }

    [HttpGet("{videoId:guid}/observations")]
    [ProducesResponseType(typeof(IReadOnlyList<ExerciseTechniqueObservationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ExerciseTechniqueObservationResultDto>>> GetVideoObservationsHistory(
        [FromRoute] Guid clientId,
        [FromRoute] Guid videoId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        var observations = await _videoAnalysisService.GetVideoObservationsHistoryAsync(coachId, clientId, videoId, cancellationToken);
        return Ok(observations);
    }
}
