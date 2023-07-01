using NLua;
using util;

using core.players;

namespace core.effects;

/// <summary>
/// Activated effect of a card
/// </summary>
class ActivatedEffect {
        
    public string Zone { get; }
    public LuaFunction CheckF { get; }
    public LuaFunction CostF { get; }
    public LuaFunction EffectF { get; }

    public ActivatedEffect(LuaTable table) {
        Zone = LuaUtility.TableGet<string>(table, "zone");
        CheckF = LuaUtility.TableGet<LuaFunction>(table, "checkF");
        CostF = LuaUtility.TableGet<LuaFunction>(table, "costF");
        EffectF = LuaUtility.TableGet<LuaFunction>(table, "effectF");
    }

    /// <summary>
    /// Execute checker function
    /// </summary>
    /// <param name="f">Checker function</param>
    /// <param name="lState">Lua state</param>
    /// <param name="player">Owner of the effect</param>
    /// <param name="args">Additional arguments</param>
    /// <returns>Returns the return value of the function</returns>
    private bool CheckFunction(LuaFunction f, Lua lState, Player player, Dictionary<string, object> args) {
        var tArgs = LuaUtility.CreateTable(lState, args);
        var returned = f.Call(player.ID, tArgs);
        return LuaUtility.GetReturnAsBool(returned);
    }

    /// <summary>
    /// Executes check function of the effect
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <param name="player">Owner of the effect</param>
    /// <param name="args">Additional arguments</param>
    /// <returns>Returns the return value of the function</returns>
    public bool ExecCheck(Lua lState, Player player, Dictionary<string, object> args) => CheckFunction(CheckF, lState, player, args);
    
    /// <summary>
    /// Executes cost function of the effect
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <param name="player">Owner of the effect</param>
    /// <param name="args">Additional arguments</param>
    /// <returns>Returns the return value of the function</returns>
    public bool ExecCosts(Lua lState, Player player, Dictionary<string, object> args) => CheckFunction(CostF, lState, player, args);

    /// <summary>
    /// Executes the actual effect
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <param name="player">Owner of the effect</param>
    /// <param name="args">Additional arguments</param>
    public void ExecEffect(Lua lState, Player player, Dictionary<string, object> args) => EffectF.Call(player.ID, LuaUtility.CreateTable(lState, args));
}

/// <summary>
/// Triggered effect of a card
/// </summary>
class Trigger : ActivatedEffect {        
    public string On { get; }
    public bool IsSilent { get; }

    public Trigger(LuaTable table) : base(table) {
        On = LuaUtility.TableGet<string>(table, "on");
        IsSilent = LuaUtility.GetBool(table, "isSilent");
    }   
}