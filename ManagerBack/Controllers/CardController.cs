using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;


[ApiController]
[Route("/api/v1/card")]
public class CardController : ControllerBase
{
    private readonly ICardService _cardService;

    public CardController(ICardService cardService) {
        _cardService = cardService;
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
            return NotFound(e.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ExpansionCard card) {
        try {
            var result = await _cardService.Create(card);
            return Ok(result);
        } catch (CIDTakenException e) {
            return BadRequest(e.Message);
        } catch (InvalidCardCreationParametersException e) {
            return BadRequest(e.Message);
        }
    }

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

    [HttpGet("name/all")]
    public async Task<IActionResult> GetAllNames() {
        return Ok( await _cardService.AllNames() );
    }
}
