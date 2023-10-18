using Microsoft.AspNetCore.Mvc;
using core.cards;
using System.Xml.Linq;
using Npgsql;

namespace manager_back.Controllers;

public class CardQuery {
    public string Name { get; set; }="";
    public string Expansion { get; set; }="";

    public bool Matches(ExpansionCard card)
    {
        if (Expansion.Length > 0 && card.Expansion.ToLower() != Expansion.ToLower()) return false;
        if (Name.Length > 0 && !card.Name.ToLower().Contains(Name.ToLower())) return false;
        // TODO add other parameters
        
        return true;
    }
}

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    //[HttpGet]
    //public IEnumerable<Card> Get([FromQuery] CardQuery query)
    //{
    //    foreach (var card in Global.CMaster.GetAll())
    //        if (query.Matches(card))
    //            yield return card;

    //}

    [HttpGet]
    public IEnumerable<CardData> GetAll()
    {
        return Global.Ctx.Cards.ToList();
    }
}
