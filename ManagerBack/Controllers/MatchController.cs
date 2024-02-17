using System.Net;
using System.Net.WebSockets;
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
    [HttpGet("connect/{matchId}")]
    public async Task Connect(string matchId) {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            // var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);
            var userId = "";

            // ? are these status codes ok
            try {
                await _matchService.Connect(HttpContext.WebSockets, userId, matchId);
            } catch (MatchNotFoundException) {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            } catch (MatchRefusedConnectionException) {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Locked;
            }
        } else {
            HttpContext.Response.StatusCode = 400;
        }
    }

    [HttpGet]
    public async Task<IActionResult> All() {
        return Ok(await _matchService.All());
    }
}