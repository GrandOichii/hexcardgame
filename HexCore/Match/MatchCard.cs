using NLua;

namespace HexCore.GameMatch;


public class MatchCard {
    static private readonly string WRAPPER_CREATION_FNAME = "_Create";
    static public readonly string CAN_PLAY_FNAME = "CanPlay";
    static public readonly string PAY_COSTS_FNAME = "PayCosts";
    static public readonly string EFFECT_FNAME = "Effect";

    public Match Match { get; }
    public Player Owner { get; set; }
    public Player OriginalOwner { get; set; }
    public string MID { get; }
    public ExpansionCard Original { get; }
    public LuaTable Data { get; }
    public bool GoesToDiscard { get; set; }=true;

    public long MaxMovement => LuaUtility.GetLong(Data, "maxMovement");
    public long Movement => LuaUtility.GetLong(Data, "movement");
    public long BaseDefence => LuaUtility.GetLong(Data, "baseDefence");
    public long Defence => LuaUtility.GetLong(Data, "defence");
    public long MaxDefence => LuaUtility.GetLong(Data, "maxDefence");
    public string Type => LuaUtility.TableGet<string>(Data, "type");
    public string Text => LuaUtility.TableGet<string>(Data, "text");
    public string BaseType { get {
        var t = Type;
        if (!t.Contains(" - ")) return t;

        var s = t.Split(" - ");
        return s[0];
    }}

    public long Power { get {
        // TODO create separate power pipeline
        return LuaUtility.GetLong(Data, "power");
    }}
    public long Cost { get {
        // TODO create separate cost pipeline
        return LuaUtility.GetLong(Data, "cost");
    }}
    public long Life => LuaUtility.GetLong(Data, "life");
    public string Name => LuaUtility.TableGet<string>(Data, "name");

    public string ToLogForm => "[[" + Name + "#" + Original.GetCID() + "]]";

    public MatchCard(Match match, ExpansionCard card, Player player) {
        var lState = match.LState;
        lState.DoString(card.Script);
        var mID = match.CardIDCreator.Next();
        var creationF = LuaUtility.GetGlobalF(lState, WRAPPER_CREATION_FNAME);
        var props = card.GetProps(lState);
        var returned = creationF.Call(props);
        var data = LuaUtility.GetReturnAs<LuaTable>(returned);
        data["id"] = mID;
        data["ownerID"] = player.ID;

        Match = match;
        MID = mID;
        Original = card;
        Data = data;

        OriginalOwner = player;
        Owner = OriginalOwner;
        data["baseType"] = BaseType;
    }

    public bool IsPlaceable => IsUnit || Original.Type == "Structure";

    /// <summary>
    /// Resets the movement of the card (if card is a Unit)
    /// </summary>
    public void ResetMovement() {
        if (MaxMovement == -1) return;

        Data["movement"] = MaxMovement;
    }

    public bool IsUnit => Type.Contains("Unit");

    public bool CanMove => Movement > 0;

    public string ShortStr => Original.Name + " [" + MID + "]";

    /// <summary>
    /// Executes card function
    /// </summary>
    /// <param name="fName">Function name</param>
    /// <param name="args">Function arguments</param>
    /// <returns>The return values of the function</returns>
    public object[] ExecFunc(string fName, params object[] args) {
        var f = LuaUtility.TableGet<LuaFunction>(Data, fName);
        return f.Call(args);
    }

    /// <summary>
    /// Executes checker function
    /// </summary>
    /// <param name="fName">Checker function name</param>
    /// <param name="args">Checker function arguments</param>
    /// <returns>The return value of the checker function</returns>
    public bool ExecCheckerFunc(string fName, params object[] args) {
        var results = ExecFunc(fName, args);
        var result = LuaUtility.GetReturnAsBool(results);
        return result;
    }

    /// <summary>
    /// Process damage, dealt to the card
    /// </summary>
    /// <param name="damage">Damage</param>
    /// <returns>Deal damage</returns>
    public long ProcessDamage(string sourceID, long damage) {
        var def = Defence;
        if (def > 0) {
            var damageDealtToDefence = damage;
            if (def < damage) {
                damageDealtToDefence = def;
            }
            Data["defence"] = def - damageDealtToDefence;
            damage -= damageDealtToDefence;
        }
        var l = Life;
        if (damage > l) damage = l;

        Data["life"] = LuaUtility.GetLong(Data, "life") - damage;
        if (damage > 0)
            Match.Emit("damage_dealt", new(){{"fromID", sourceID}, {"toID", MID}, {"amount", damage}});

        return damage;
    }


    public bool CanBePlayed(Player player) {
        var result = ExecCheckerFunc(CAN_PLAY_FNAME, Data, player.ID);
        return result;
    }
}
