using System.Text.Json;
using System.Text.Json.Serialization;
using NLua;

namespace HexCore.Cards;

/// <summary>
/// Card object, for storage in database
/// </summary>
public class Card
{
    public required string Name { get; set; }
    public required int Cost { get; set; }
    public required string Type { get; set; }
    public required string Text { get; set; }
    public int Power { get; set; } = -1;
    public int Life { get; set; } = -1;
    public bool DeckUsable { get; set; } = true;
    public required string Script { get; set; }

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

/// <summary>
/// Represents card data that has an expansion
/// </summary>
public class ExpansionCard : Card {
    public required string Expansion { get; set; }

    /// <summary>
    /// Returns ID of the card in the format of [expansion]::[name].
    /// </summary>
    /// <returns>ID of the card</returns>
    // public string CID => Expansion + "::" + Name;
    public string GetCID() => Expansion + "::" + Name;
}