using core.decks;
using core.match;
using core.manager;
using Microsoft.AspNetCore.Mvc;

namespace manager_back.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ConfigsController : ControllerBase
{
    [HttpGet]
    public IEnumerable<ManagerMatchConfig> Get()
    {
        return Global.CManager.Configs;
    }

}
