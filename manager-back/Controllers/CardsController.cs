using Microsoft.AspNetCore.Mvc;
using core.cards;
using System.Xml.Linq;
using Npgsql;
using Microsoft.EntityFrameworkCore;

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
        // TODO is this ok?
        var ctx = Global.Ctx;
        var result = ctx.Cards.ToList();
        
        foreach (var card in result)
        {
            card.Expansions = ctx.ExpansionCards.Where(c => c.CardNameKey == card.Name).Select(c => c.Expansion).ToList();
        }
        
        return result;
    }

    [HttpPost]
    public IActionResult Put([FromBody] CardData card)
    {
        var ctx = Global.Ctx;
        // check that a card with the same name doens't already exists
        var checkCard = ctx.Cards.SingleOrDefault(c => c.Name == card.Name);
        if (checkCard is not null)
        {
            return StatusCode(400, "Card with name " + card.Name + " already exists");
        }

        // update card
        ctx.Cards.Add(card);

        ctx.SaveChanges();
        return Ok(card);
    }

    [HttpPut]
    public IActionResult Put(string oldName, [FromBody] CardData card)
    {
        Console.WriteLine(oldName + "  " + card.Name);
        var ctx = Global.Ctx;
        // TODO could be better
     
        // check that a card with the same name doens't already exists
        var checkCard = ctx.Cards.SingleOrDefault(c => c.Name == card.Name);
        if (card.Name != oldName && checkCard is not null)
        {
            return StatusCode(400, "Card with name " + card.Name + " already exists");
        }

        //var i = 0;
        var oldCard = ctx.Cards.SingleOrDefault(c => c.Name == oldName);

        // delete old record if exists
        if (oldCard is not null)
        {
            ctx.Cards.Remove(oldCard);
        }

        // update card
        ctx.Cards.Add(card);

        // update expansion cards
        foreach (var e in card.Expansions)
        {
            var n = new ExpansionCardData();
            n.ExpansionNameKey = e.Name;
            n.CardNameKey = card.Name;
            ctx.ExpansionCards.Add(n);
        }

        // commit changes
        ctx.SaveChanges();
        return Ok(card);
    }
}
