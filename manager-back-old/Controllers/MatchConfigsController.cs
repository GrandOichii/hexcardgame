using Microsoft.AspNetCore.Mvc;
using core.cards;
using System.Xml.Linq;
using Npgsql;

namespace manager_back.Controllers;


[ApiController]
[Route("api/matchconfigs")]
public class MatchConfigsController : ControllerBase
{
    [HttpGet]
    public IEnumerable<MatchConfigData> GetAll()
    {
        return Global.Ctx.MatchConfigs.ToList();
    }
}
