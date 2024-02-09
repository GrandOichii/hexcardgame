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

    // TODO
    [HttpPost("create")]
    public async Task Create() {

    }
}