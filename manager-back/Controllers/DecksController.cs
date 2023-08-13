using Microsoft.AspNetCore.Mvc;
using core.cards;
using core.decks;
using System.Xml.Linq; 

namespace manager_back.Controllers;


[ApiController]
[Route("api/[controller]")]
public class DecksController : ControllerBase
{
    [HttpGet]
    public IEnumerable<DeckTemplate> Get()
    {
        return Global.DManager.Decks;
    }

    [HttpGet("random")]
    public DeckTemplate GetRandom() {
        // TODO
        return new DeckTemplate();
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