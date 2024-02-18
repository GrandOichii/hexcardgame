using System.Net;
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
    // [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MatchProcessConfig config) {
        // var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);
        var userId = "";

        var match = await _matchService.Create(userId, config);
        return Ok(match.Id);
    }

    // TODO authorize
    // [Authorize]
    [HttpGet("wsconnect/{matchId}")]
    public async Task WebSocketConnect(string matchId) {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            // var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);
            var userId = "";

            // ? are these status codes ok
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

    // TODO authorize
    // [Authorize]
    [HttpGet("tcpconnect/{matchId}")]
    public async Task TCPConnect(string matchId) {
        // var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);
        var userId = "";

        try {
            await _matchService.TCPConnect(userId, matchId);
        } catch (InvalidMatchIdException) {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest; 
        } catch (MatchNotFoundException) {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
        } catch (MatchRefusedConnectionException) {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
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
}