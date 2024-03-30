using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;

[ApiController]
[Route("/api/v1/match")]
public class MatchController : ControllerBase {

    private readonly IMatchService _matchService;

    public MatchController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    // TODO authorize
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MatchProcessConfig config) {
        var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);
        try {
            var match = await _matchService.Create(userId, config);
            return Ok(match);
        } catch (MatchConfigNotFoundException e) {
            return NotFound(e.Message);
        }
    }

    // TODO authorize
    // [Authorize]
    [HttpGet("connect/{matchId}")]
    public async Task WebSocketConnect(string matchId) {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            // var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);
            var userId = "";

            try {
                await _matchService.WSConnect(HttpContext.WebSockets, userId, matchId);
            } catch (InvalidMatchIdException) {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest; 
            } catch (MatchNotFoundException) {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            } catch (MatchRefusedConnectionException) {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        } else {
            HttpContext.Response.StatusCode = 400;
        }
    }

    [HttpGet]
    public async Task<IActionResult> All() {
        return Ok(await _matchService.All());
    }

    [HttpGet("{matchId}")]
    public async Task<IActionResult> ById(string matchId) {
        try {
            return Ok(await _matchService.ById(matchId));
        } catch (InvalidMatchIdException e) {
            return BadRequest(e.Message);
        } catch (MatchNotFoundException e) {
            return NotFound(e.Message);
        } catch (MatchRefusedConnectionException e) {
            return BadRequest(e.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("crashed")]
    public async Task<IActionResult> RemoveCrashed() {
        await _matchService.Remove(m => m.Status == MatchStatus.CRASHED);
        return await All();
    }
}