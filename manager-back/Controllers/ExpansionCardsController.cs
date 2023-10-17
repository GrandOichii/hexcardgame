using Microsoft.AspNetCore.Mvc;
using core.cards;
using System.Xml.Linq;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace manager_back.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ExpansionCardsController : ControllerBase
{
    //[HttpGet]
    //public IEnumerable<Card> Get([FromQuery] CardQuery query)
    //{
    //    foreach (var card in Global.CMaster.GetAll())
    //        if (query.Matches(card))
    //            yield return card;

    //}

    [HttpGet]
    public IEnumerable<ExpansionCardData> GetAll()
    {
        return Global.Ctx.ExpansionCards
            .Include(c => c.Expansion)
            .Include(c => c.Card)
            .ToList();
    }
}
