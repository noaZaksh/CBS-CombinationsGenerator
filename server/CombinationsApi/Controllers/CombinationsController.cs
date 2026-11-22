using CombinationsCore.Models;
using CombinationsCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace CombinationsApi.Controllers;

/// <summary>
/// The HTTP layer only: reads the request, calls the service, returns the result.
/// No calculations happen here.
/// </summary>
[ApiController]
[Route("api/combinations")]
public class CombinationsController : ControllerBase
{
    private readonly ICombinationsService _service;

    public CombinationsController(ICombinationsService service)
    {
        _service = service;
    }

    /// <summary>StartAPI – receives n and returns how many combinations exist.</summary>
    [HttpPost("start")]
    public ActionResult<StartResponse> Start([FromBody] StartRequest request)
    {
        return Ok(_service.Start(request.N));
    }

    /// <summary>GetNextAPI – returns the next combination and its serial number.</summary>
    [HttpPost("{sessionId:guid}/next")]
    public ActionResult<NextResponse> GetNext(Guid sessionId)
    {
        return Ok(_service.GetNext(sessionId));
    }

    /// <summary>GetAllAPI – returns only the combinations of the requested page.</summary>
    [HttpGet("{sessionId:guid}/all")]
    public ActionResult<PageResponse> GetAll(Guid sessionId, [FromQuery] int pageSize, [FromQuery] long pageNumber = 1)
    {
        return Ok(_service.GetAll(sessionId, pageSize, pageNumber));
    }

    /// <summary>"Back" button – the last combination shown in the list becomes the current one.</summary>
    [HttpPost("{sessionId:guid}/back")]
    public ActionResult<NextResponse> Back(Guid sessionId)
    {
        return Ok(_service.Back(sessionId));
    }
}

/// <summary>Body of the Start request.</summary>
public record StartRequest(int N);
