using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;

/// <summary>
/// Api controller for fetching card expansions
/// </summary>
[ApiController]
[Route("/api/v1/expansion")]
public class ExpansionController : ControllerBase {
    /// <summary>
    /// Expansion service
    /// </summary>
    private readonly IExpansionService _expansionService;

    public ExpansionController(IExpansionService expansionService)
    {
        _expansionService = expansionService;
    }

    /// <summary>
    /// Endpoint for fetching all expansions and their associated card counts
    /// </summary>
    /// <returns>Response object with a expansion to card count mapping as data</returns>
    [HttpGet]
    public async Task<IActionResult> All() {
        return Ok(await _expansionService.All());
    }

    /// <summary>
    /// Endpoint for fetching a specific expansion
    /// </summary>
    /// <param name="name">Expansion name</param>
    /// <returns>Response object with the expansion information as data</returns>
    [HttpGet("{name}")]
    public async Task<IActionResult> ByName(string name) {
        try {
            return Ok(await _expansionService.ByName(name));
        } catch (ExpansionNotFoundException e) {
            return BadRequest(e.Message);
        }
    }
}