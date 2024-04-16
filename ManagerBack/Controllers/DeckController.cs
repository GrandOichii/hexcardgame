using System.Security.Claims;
using ManagerBack.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;


[ApiController]
[Route("/api/v1/deck")]
public class DeckController : ControllerBase {
    private readonly IDeckService _deckService;

    public DeckController(IDeckService deckService)
    {
        _deckService = deckService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> All() {
        var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);

        return Ok(await _deckService.All(userId));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] PostDeckDto deck) {
        var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);

        try {

            var result = await _deckService.Create(userId, deck);
            return Ok(result);

        } catch (InvalidDeckException e) {
            return BadRequest(e.Message);
        } catch (InvalidCIDException e) {
            return BadRequest(e.Message);
        } catch (CardNotFoundException e) {
            return NotFound(e.Message);
        } catch (DeckAmountLimitException e) {
            return BadRequest(e.Message);
        } catch (UserNotFoundException e) {
            return BadRequest(e.Message);
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id) {
        var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);

        try {
            await _deckService.Delete(userId, id);
            return Ok();
        } catch(DeckNotFoundException e) {
            return BadRequest(e.Message);
        } catch(UnmatchedUserIdException e) {
            return BadRequest(e.Message);
        } catch(UserNotFoundException e) {
            return BadRequest(e.Message);
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] PostDeckDto deck) {
        var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);

        try {
            var result = await _deckService.Update(userId, id, deck);
            return Ok(result);
        } catch(DeckNotFoundException e) {
            return BadRequest(e.Message);
        } catch(UnmatchedUserIdException e) {
            return BadRequest(e.Message);
        } catch(UserNotFoundException e) {
            return BadRequest(e.Message);
        }
    }
}