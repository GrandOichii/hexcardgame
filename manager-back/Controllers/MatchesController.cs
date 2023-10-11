using Microsoft.AspNetCore.Mvc;
using core.cards;
using core.decks;
using System.Xml.Linq;
using core.manager;
using core.match;
using core.players;
using System.Net.Sockets;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;

namespace manager_back.Controllers;


[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    [HttpGet]
    public IEnumerable<MatchTrace> Get()
    {
        return Global.MatchTraces;
    }

    [HttpPost]
    public IEnumerable<MatchTrace> NewMatch([FromBody] MatchCreationConfig mConfig)
    {
        var results = new List<MatchTrace>();
        for (int bI = 0; bI < mConfig.Batch; bI++)
        {
            var config = mConfig.Config;
            var mCreator = MatchMaster.Instance;

            var match = mCreator.New(Global.CMaster, config);
            var id = match.ID;
            match.StrictMode = false;
            match.AllowCommands = true;

            var result = new MatchTrace();

            result.Match = match;
            result.ID = id;
            result.URL = "-";
            result.WinnerName = "";
            result.Listener = new TcpListener(IPAddress.Loopback, 0);
            result.Listener.Start();

            result.Task = Task.Run(() =>
            {
                var trace = result;
                string p = ((IPEndPoint)trace.Listener.LocalEndpoint).ToString();
                // config players
                var controllers = new PlayerController[mConfig.Players.Count];
                var realPlayerTasks = new List<Task>();
                for (int i = 0; i < mConfig.Players.Count; i++)
                {
                    var pC = mConfig.Players[i];
                    var pI = i;
                    if (pC.IsBot)
                    {
                        controllers[pI] = new LuaPlayerController("../bots/basic.lua");
                        continue;
                    }
                    result.URL = p;
                    realPlayerTasks.Add(Task.Run(() =>
                    {
                        var controller = new TCPPlayerController(trace.Listener, match);
                        controllers[pI] = controller;
                    }));
                }

                Task.WaitAll(realPlayerTasks.ToArray());
                result.URL = "-";

                // create players
                for (int i = 0; i < controllers.Length; i++)
                {
                    var controller = controllers[i];
                    var player = new Player(trace.Match, mConfig.Players[i].Name, DeckTemplate.FromText(mConfig.Players[i].DeckList), controller);
                }

                trace.Status = MatchTraceStatus.InProgress;

                try
                {
                    match.Start();
                    trace.Status = MatchTraceStatus.Finished;
                    result.WinnerName = match.Winner.Name;
                } catch (Exception e)
                {
                    trace.Status = MatchTraceStatus.Crashed;
                }
            });

            Global.MatchTraces.Add(result);
            results.Add(result);
        }

        return results;
    }
}
