using System.Text.Json;
using System.Text.Json.Serialization;
using core.match;
using util;
using NLua;
using core.players;

namespace core.cards;


/// <summary>
/// Card object, for storage in database
/// </summary>
public class Card
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("cost")]
    public int Cost { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }="";
    // [JsonPropertyName("expansion")]
    // public string Expansion { get; set; }="<no-expansion>";
    [JsonPropertyName("text")]
    public string Text { get; set; }="";
    [JsonPropertyName("power")]
    public int Power { get; set; } = -1;
    [JsonPropertyName("life")]
    public int Life { get; set; } = -1;
    [JsonPropertyName("deckUsable")]
    public bool DeckUsable { get; set; } = true;
    [JsonPropertyName("script")]
    public string Script { get; set; }="error(\"NO CARD SCRIPT SPECIFIED\")";

    /// <summary>
    /// Returns ID of the card in the format of [expansion]::[name].
    /// </summary>
    /// <returns>ID of the card</returns>
    // public string CID => Expansion + "::" + Name;


    /// <summary>
    /// Creates a table to be used in the creation function
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <returns>Props table</returns>
    public LuaTable GetProps(Lua lState) {
        var result = LuaUtility.CreateTable(lState);
        result["name"] = Name;
        result["cost"] = Cost;
        result["type"] = Type;
        result["power"] = Power;
        result["life"] = Life;
        result["text"] = Text;
        return result;
    }


    public virtual string ToJson() {
        return JsonSerializer.Serialize(this);
    }
}

public class ExpansionCard : Card {
    [JsonPropertyName("expansion")]
    public string Expansion { get; set; }

    public string CID => Expansion + "::" + Name;
}


/// <summary>
/// Card master entity, is used for card fetching
/// </summary>
public abstract class CardMaster
{
    /// <summary>
    /// Fetches the card with the specified card ID.
    /// </summary>
    /// <param name="id">Card ID</param>
    /// <returns>Card with the specified ID</returns>
    /// 
    abstract public ExpansionCard Get(string id);
    
    /// <summary>
    /// Fetches all cards
    /// </summary>
    /// <returns><Container of all cards/returns>
    abstract public IEnumerable<ExpansionCard> GetAll();
}


/// <summary>
/// File card master, loads cards using a manifest file
/// </summary>
// public class FileCardMaster : CardMaster
// {
//     private static string MANIFEST_FILE = "manifest.json";

//     public List<Card> Cards { get; }

//     public FileCardMaster() {
//         Cards = new();
//     }

//     /// <summary>
//     /// Loads the cards from the specified directory. Has to contain a manifest.json file.
//     /// </summary>
//     /// <param name="dir">Directory of the cards.</param>
//     /// <returns>The amount of cards loaded.</returns>
//     public int LoadCardsFrom(string dir) {
//         // read the manifest file
//         var manifestFile = Path.Join(dir, MANIFEST_FILE);
//         var manifest = File.ReadAllText(manifestFile);  
//         var cardDirs = JsonSerializer.Deserialize<List<string>>(manifest);
//         if (cardDirs is null) {
//             throw new Exception("Failed to load manifest file in " + manifestFile);
//         }

//         int result = 0;
//         foreach (var cardDir in cardDirs) {
//             if (cardDir[0] == '!') continue;
            
//             var cardPath = Path.Join(dir, cardDir);
//             var text = File.ReadAllText(cardPath);
//             var card = JsonSerializer.Deserialize<Card>(text);
//             if (card is null) {
//                 throw new Exception("Failed to deserialize card from " + cardPath);
//             }

//             Cards.Add(card);
//             ++result;
//         }
//         return result;
//     }

//     public override ExpansionCard Get(string id)
//     {
//         foreach (var card in Cards)
//             if (card.CID == id)
//                 return card;

//         throw new Exception("Can't load card with ID " + id);
//     }

//     public override IEnumerable<Card> GetAll() => Cards;
// }


public class MCard {
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

    public string ToLogForm => "[[" + Name + "#" + Original.CID + "]]";

    public MCard(Match match, ExpansionCard card, Player player) {
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
