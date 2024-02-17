﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Core.GameMatch;
using Util;
using NLua;

namespace Core.Cards;


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

