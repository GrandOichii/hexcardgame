using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;


public class CardQuery {
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Text { get; set; } = "";
    public string Expansion { get; set; } = "";
}


[ApiController]
[Route("/api/v1/card")]
public class CardController : ControllerBase
{
    private readonly ICardService _cardService;

    public CardController(ICardService cardService) {
        _cardService = cardService;
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

    [HttpGet("cid/all")]
    public async Task<IActionResult> GetAllCIDs() {
        return Ok( await _cardService.AllCIDs() );
    }

    [HttpGet]
    public async Task<IActionResult> Query([FromQuery] CardQuery query) {
        return Ok(await _cardService.Query(query) );
    }
}
