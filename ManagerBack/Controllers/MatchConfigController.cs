using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ManagerBack.Controllers;

/// <summary>
/// Api controller for managing match configurations
/// </summary>
[ApiController]
[Route("/api/v1/config")]
public class MatchConfigController : ControllerBase {
    /// <summary>
    /// Match configuration service
    /// </summary>
    private readonly IMatchConfigService _configService;

    public MatchConfigController(IMatchConfigService configService)
    {
        _configService = configService;
    }

    /// <summary>
    /// Admin-only endpoint for fetching all match configurations
    /// </summary>
    /// <returns>Response object with all configurations as data</returns>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> All() {
        return Ok(await _configService.All());
    }

    /// <summary>
    /// Admin-only endpoint for creating new match configurations
    /// </summary>
    /// <param name="config">Match configuration data</param>
    /// <returns>Response object with the new match configuration information as data</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PostMatchConfigDto config) {
        try {
            var result = await _configService.Add(config);
            return Ok(result);
        } catch (InvalidMatchConfigCreationParametersException e) {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Admin-only endpoint for fetching a match configurationm be it's id
    /// </summary>
    /// <param name="id">Match configuration ID</param>
    /// <returns>Response object with the match configuration information as data</returns>
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> ById(string id) {
        try {
            var result = await _configService.ById(id);
            return Ok(result);
        } catch (MatchConfigNotFoundException e) {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Endpoint for fetching the basic match configuration
    /// </summary>
    /// <returns>Response object with the match configuration ID as data</returns>
    [HttpGet("basic")]
    public async Task<IActionResult> Basic() {
        try {
            var result = await _configService.Basic();
            return Ok(result.Id);
        } catch (NoBasicMatchConfigException) {
            throw;
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] PostMatchConfigDto config) {
        try {
            var result = await _configService.Update(config);
            return Ok(result);
        } catch (InvalidMatchConfigCreationParametersException e) {
            return BadRequest(e.Message);
        } catch (MatchConfigNotFoundException e) {
            return BadRequest(e.Message);
        }
    }
}