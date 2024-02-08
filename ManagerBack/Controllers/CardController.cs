using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;


[ApiController]
[Route("/api/v1/card")]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;

    public CardsController(ICardService cardService) {
        _cardService = cardService;
    }

    // TODO remove
    [HttpGet("all")]
    public async Task<IActionResult> All() {
        return Ok(await _cardService.All());
    }

    [HttpGet("fromexpansion/{expansion}")]
    public async Task<IActionResult> FromExpansion(string expansion) {
        return Ok(await _cardService.ByExpansion(expansion));
    }

    [HttpGet("{cid}")]
    public async Task<IActionResult> ByCID(string cid) {
        try {
            var result = await _cardService.ByCID(cid);
            return Ok(result);
        } catch (InvalidCIDException e) {
            return BadRequest(e.Message);
        } catch (CardNotFoundException e) {
            return BadRequest(e.Message);
        }
    }

    // TODO authorize
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] ExpansionCard card) {
        try {
            var result = await _cardService.Create(card);
            return Ok(result);
        } catch (CIDTakenException e) {
            return BadRequest(e.Message);
        }
    }

    // TODO authorize
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete(string cid) {
        try {
            await _cardService.Delete(cid);
            return Ok();
        } catch (InvalidCIDException e) {
            return BadRequest(e.Message);
        }
    }
}