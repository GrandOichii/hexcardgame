using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;

/// <summary>
/// Query object used to fetch cards
/// </summary>
public class CardQuery {
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Text { get; set; } = "";
    public string Expansion { get; set; } = "";
}

/// <summary>
/// Api controller for managing cards
/// </summary>
[ApiController]
[Route("/api/v1/card")]
public class CardController : ControllerBase
{
    /// <summary>
    /// Card service
    /// </summary>
    private readonly ICardService _cardService;

    public CardController(ICardService cardService) {
        _cardService = cardService;
    }

    /// <summary>
    /// Endpoint for fetching a card with the specified CID
    /// </summary>
    /// <param name="cid">Card ID</param>
    /// <returns>Response object with the card as data</returns>
    [HttpGet("{cid}")]
    public async Task<IActionResult> ByCID(string cid) {
        try {
            var result = await _cardService.ByCID(cid);
            return Ok(result);
        } catch (InvalidCIDException e) {
            return BadRequest(e.Message);
        } catch (CardNotFoundException e) {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Admin-only endpoint for creating new cards
    /// </summary>
    /// <param name="card">Card data</param>
    /// <returns>Response object with the card as data</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ExpansionCard card) {
        try {
            var result = await _cardService.Add(card);
            return Ok(result);
        } catch (CIDTakenException e) {
            return BadRequest(e.Message);
        } catch (InvalidCardCreationParametersException e) {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Admin-only endpoint for deleting existing cards
    /// </summary>
    /// <param name="cid">Card ID</param>
    /// <returns>Response object</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{cid}")]
    public async Task<IActionResult> Delete(string cid) {
        try {
            await _cardService.Delete(cid);
            return Ok();
        } catch (CardNotFoundException e) {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Admin-only endpoint for updating existing cards
    /// </summary>
    /// <param name="card">New card data</param>
    /// <returns>Response object with the new card information as data</returns>
    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ExpansionCard card) {
        try {
            await _cardService.Update(card);
            return Ok();
        } catch (CardNotFoundException e) {
            return NotFound(e.Message);
        } catch (InvalidCardCreationParametersException e) {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Endpoint for fetching all CIDs
    /// </summary>
    /// <returns>Response object with the CID list as data</returns>
    [HttpGet("cid/all")]
    public async Task<IActionResult> GetAllCIDs() {
        return Ok( await _cardService.AllCIDs() );
    }

    /// <summary>
    /// Endpoint for querying cards
    /// </summary>
    /// <param name="query">Card query</param>
    /// <returns>Response object with the queried cards as data</returns>
    [HttpGet]
    public async Task<IActionResult> Query([FromQuery] CardQuery query) {
        return Ok(await _cardService.Query(query) );
    }
}
