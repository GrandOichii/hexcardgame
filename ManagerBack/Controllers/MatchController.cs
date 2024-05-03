using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;

/// <summary>
/// Api controller for managing match instances
/// </summary>
[ApiController]
[Route("/api/v1/match")]
public class MatchController : ControllerBase {
    /// <summary>
    /// Match process service
    /// </summary>
    private readonly IMatchService _matchService;

    public MatchController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    /// <summary>
    /// Endpoint for creating a new match
    /// </summary>
    /// <param name="config">Match configuration</param>
    /// <returns>Response object with the match information as data</returns>
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

    /// <summary>
    /// Websocket endpoint for connecting to an existing match
    /// </summary>
    /// <param name="matchId">Match ID</param>
    /// <returns>REsponse object</returns>
    [Authorize]
    [HttpGet("connect/{matchId}")]
    public async Task WebSocketConnect(string matchId) {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);
            // var userId = "";

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

    /// <summary>
    /// Endpoint for fetching all match instances
    /// </summary>
    /// <returns>Response object with all matches as data</returns>
    [HttpGet]
    public async Task<IActionResult> All() {
        return Ok(await _matchService.All());
    }

    /// <summary>
    /// Endpoint for fetching a match by it's ID
    /// </summary>
    /// <param name="matchId">Match ID</param>
    /// <returns>Response object with the match information as data</returns>
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

    /// <summary>
    /// Admin-only endpoint for removing all crashed matches
    /// </summary>
    /// <returns>Response object</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("crashed")]
    public async Task<IActionResult> RemoveCrashed() {
        await _matchService.Remove(m => m.Status == MatchStatus.CRASHED);
        return await All();
    }
}