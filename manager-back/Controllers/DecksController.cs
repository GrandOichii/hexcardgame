using Microsoft.AspNetCore.Mvc;
using core.cards;
using core.decks;
using System.Xml.Linq;
using System.Text.Json.Serialization;
using System.Net.Mail;

namespace manager_back.Controllers;

//public class DeckModification
//{
//    [JsonPropertyName("cid")]
//    public string CID { get; set; }
//    [JsonPropertyName("mod")]
//    public int Mod { get; set; }
//}


[ApiController]
[Route("api/[controller]")]
public class DecksController : ControllerBase
{
    [HttpGet]
    public IEnumerable<DeckTemplate> Get()
    {
        return Global.DManager.Decks;
    }

    //[HttpGet("random")]
    //public DeckTemplate GetRandom() {
    //    // TODO
    //    return new DeckTemplate();
    //}

    [HttpPost]
    public DeckTemplate CreateNew([FromBody] DeckTemplate deck)
    {
        // TODO check if deck with that name already exists?
        Global.DManager.Decks.Add(deck);
        // TODO save decks
        return deck;
    }


    [HttpPut]
    public List<DeckTemplate> ModifyDeck([FromBody] List<DeckTemplate> newDecks)
    {
        // TODO don't know if this is the best way of doing this
        Global.DManager.Decks = newDecks;
        return Global.DManager.Decks;
    }
    
}


// - create back + front for managing cards
//     - has to: manage cards, 
//               manage expansions, 
//               manage decks, 
//               generate decks, 
//               manage matches, 
//               replaying recorded matches, 
//               manage match configurations