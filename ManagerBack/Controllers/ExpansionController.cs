using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;

[ApiController]
[Route("/api/v1/expansion")]
public class ExpansionController : ControllerBase {
    private readonly IExpansionService _expansionService;

    public ExpansionController(IExpansionService expansionService)
    {
        _expansionService = expansionService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> All() {
        return Ok(await _expansionService.All());
    }
}