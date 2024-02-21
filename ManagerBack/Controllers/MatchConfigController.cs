using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;


[ApiController]
[Route("/api/v1/config")]
public class MatchConfigController : ControllerBase {
    private readonly IMatchConfigService _configService;

    public MatchConfigController(IMatchConfigService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public async Task<IActionResult> All() {
        return Ok(await _configService.All());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MatchConfig config) {
        try {
            var result = await _configService.Create(config);
            return Ok(result);
        } catch (InvalidMatchConfigCreationParametersException e) {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ById(string id) {
        try {
            var result = await _configService.ById(id);
            return Ok(result);
        } catch (MatchConfigNotFoundException e) {
            return NotFound(e.Message);
        }
    }

    // [Authorize(Roles = "Admin")]
    // [HttpPut]
    // public async Task<IActionResult> Update([FromBody] ExpansionCard card) {
    //     try {
    //         await _configService.Update(card);
    //         return Ok();
    //     } catch (CardNotFoundException e) {
    //         return BadRequest(e.Message);
    //     } catch (InvalidCardCreationParametersException e) {
    //         return BadRequest(e.Message);
    //     }
    // }

}