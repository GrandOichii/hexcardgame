using Microsoft.AspNetCore.Mvc;
using core.cards;
using System.Xml.Linq;
using Npgsql;

namespace manager_back.Controllers;


[ApiController]
[Route("api/expansions")]
public class ExpansionsController : ControllerBase
{
    //[HttpGet]
    //public IEnumerable<Card> Get([FromQuery] CardQuery query)
    //{
    //    foreach (var card in Global.CMaster.GetAll())
    //        if (query.Matches(card))
    //            yield return card;

    //}

    [HttpGet]
    public IEnumerable<ExpansionData> GetAll()
    {
        return Global.Ctx.Expansions.ToList();
    }
}
