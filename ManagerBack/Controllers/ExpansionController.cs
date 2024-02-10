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

    [HttpGet]
    public async Task<IActionResult> All() {
        return Ok(await _expansionService.All());
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> ByName(string name) {
        try {
            return Ok(await _expansionService.ByName(name));
        } catch (ExpansionNotFoundException e) {
            return BadRequest(e.Message);
        }
    }
}