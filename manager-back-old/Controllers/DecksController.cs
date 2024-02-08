using Microsoft.AspNetCore.Mvc;
using core.cards;
using core.decks;
using System.Xml.Linq;
using System.Text.Json.Serialization;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;

namespace manager_back.Controllers;

//public class DeckModification
//{
//    [JsonPropertyName("cid")]
//    public string CID { get; set; }
//    [JsonPropertyName("mod")]
//    public int Mod { get; set; }
//}


[ApiController]
[Route("api/decks")]
public class DecksController : ControllerBase
{
    [HttpGet]
    public IEnumerable<DeckData> Get()
    {
        var result = Global.Ctx.Decks.ToList();
        foreach (var deck in result)
        {
            deck.Cards = Global.Ctx.DeckCards
                .Where(d => d.DeckNameKey == deck.Name)
                .Include(c => c.Card)
                    .ThenInclude(c => c.Card)
                .ToList();
        }
        return result.ToList();
    }

    [HttpPut]
    public IActionResult Put(string oldName, [FromBody] DeckData deck)
    {
        var ctx = Global.Ctx;
        // TODO could be better

        // check that a deck with the same name doesn't already exist
        var checkDeck = ctx.Decks.SingleOrDefault(d => d.Name == deck.Name);
        if (deck.Name != oldName && checkDeck is not null)
        {
            return StatusCode(400, "Deck with name " + deck.Name + " already exists");
        }

        //var i = 0;
        var oldDeck = ctx.Decks.SingleOrDefault(c => c.Name == oldName);

        // delete old record if exists
        if (oldDeck is not null)
        {
            ctx.Decks.Remove(oldDeck);
        }

        ctx.SaveChanges();

        // update deck
        ctx.Decks.Add(deck);

        // update cards
        foreach (var card in deck.Cards)
        {
            ctx.DeckCards.Add(card);
        }

        // commit changes
        ctx.SaveChanges();
        return Ok(deck);
    }
}
