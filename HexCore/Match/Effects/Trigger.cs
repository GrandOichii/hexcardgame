using NLua;

namespace HexCore.GameMatch.Effects;

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