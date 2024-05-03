using System.Security.Claims;
using ManagerBack.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;

/// <summary>
/// Api controller for managing user-created decks
/// </summary>
[ApiController]
[Route("/api/v1/deck")]
public class DeckController : ControllerBase {
    /// <summary>
    /// Deck service
    /// </summary>
    private readonly IDeckService _deckService;

    public DeckController(IDeckService deckService)
    {
        _deckService = deckService;
    }

    /// <summary>
    /// Endpoint for fetching all user-created decks
    /// </summary>
    /// <returns>Response object with the user's decks as data</returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> All() {
        var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);

        return Ok(await _deckService.All(userId));
    }

    /// <summary>
    /// Endpoint for creating a new user deck
    /// </summary>
    /// <param name="deck">Deck data</param>
    /// <returns>Response object with the new deck's info as data</returns>
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

    /// <summary>
    /// Endpoint for deleting a user deck
    /// </summary>
    /// <param name="id">Deck ID</param>
    /// <returns>Response object</returns>
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

    /// <summary>
    /// Endpoint for updating user decks
    /// </summary>
    /// <param name="id">Deck id</param>
    /// <param name="deck">New deck data</param>
    /// <returns>Response object with the new deck information as data</returns>
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