using Microsoft.AspNetCore.Mvc;
using core.cards;
using System.Xml.Linq; 

namespace manager_back.Controllers;

public class CardQuery {
    public string Name { get; set; }="";
    public string Expansion { get; set; }="";

    public bool Matches(Card card)
    {
        if (Expansion.Length > 0 && card.Expansion.ToLower() != Expansion.ToLower()) return false;
        if (Name.Length > 0 && card.Name.ToLower() != Name.ToLower()) return false;
        
        return true;
    }
}

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    [HttpGet]
    public IEnumerable<Card> Get([FromQuery] CardQuery query)
    {
        foreach (var card in Global.CMaster.GetAll())
            if (query.Matches(card))
                yield return card;

    }
}