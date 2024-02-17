using NLua;
using Util;

namespace Core.GameMatch.Players.Controllers;


/// <summary>
/// A player controller, whose actions are controlled by a lua script
/// </summary>
public class LuaPlayerController : IPlayerController {
    static private readonly string SETUP_FNAME = "_Setup";
    static private readonly string PROMPT_ACTION_FNAME = "_PromptAction";
    static private readonly string UPDATE_FNAME = "_Update";
    static private readonly string CLEANUP_FNAME = "_Cleanup";

    private readonly string _sPath;
    private readonly Lua LState;

    private readonly LuaFunction _setupF;
    private readonly LuaFunction _promptActionF;
    private readonly LuaFunction _updateF;
    private readonly LuaFunction _cleaupF;
    public LuaPlayerController(string sPath) {
        _sPath = sPath;

        LState = new();
        LState.DoFile(_sPath);
        _setupF = LuaUtility.GetGlobalF(LState, SETUP_FNAME);
        _promptActionF = LuaUtility.GetGlobalF(LState, PROMPT_ACTION_FNAME);
        _updateF = LuaUtility.GetGlobalF(LState, UPDATE_FNAME);
        _cleaupF = LuaUtility.GetGlobalF(LState, CLEANUP_FNAME);
    }   

    public string DoPromptAction(Player player, Match match)
    {        
        var result = _promptActionF.Call(new MatchState(match, player, "action").ToJson());
        return LuaUtility.GetReturnAs<string>(result);
    }

    public void Setup(Player player, Match match)
    {

        _setupF.Call(new MatchState(match, player, "setup").ToJson());
    }

    public void Update(Player player, Match match)
    {
        _updateF.Call(new MatchState(match, player, "update").ToJson());
    }

    public void CleanUp() {
        _cleaupF.Call();
    }

    public string DoPickTile(List<int[]> choices, Player player, Match match)
    {
        // TODO
        return "" + choices[0][0] + "." + choices[1][0];
    }

    public void SendCard(Match match, Player player, ExpansionCard card)
    {
    }
}
