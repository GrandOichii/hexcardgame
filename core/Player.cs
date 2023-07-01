using core.cards;
using core.decks;
using core.match;
using NLua;
using util;

namespace core.players;

/// <summary>
/// Controller class, handles actions of player
/// </summary>
abstract public class PlayerController {
    /// <summary>
    /// Prompts the action from player
    /// </summary>
    /// <param name="player">Controller player</param>
    /// <param name="match">Match</param>
    /// <returns>Action of the player</returns>
    public string PromptAction(Player player, Match match) {
        var result = DoPromptAction(player, match);
        // TODO
        return result;
    }

    abstract public string DoPromptAction(Player player, Match match);
}


/// <summary>
/// In-match player object.
/// </summary>
public class Player {
    private Match _match;
    public string Name { get; }
    public PlayerController Controller { get; }
    public string ID { get; }

    public int Energy { get; set; }

    public Dictionary<MCard, string> AllCards { get; }

    // zones
    public Zone<MCard> Deck { get; }
    public Zone<MCard> Discard { get; }
    public Zone<MCard> Hand { get; }


    public Player(Match match, string name, DeckTemplate dTemplate, PlayerController controller) {
        _match = match;
        Name = name;
        Controller = controller;

        match.Players.Add(this);
        ID = match.PlayerIDCreator.Next();

        // zones
        Deck = dTemplate.ToDeck(match, this);
        Deck.Shuffle(match.Rnd);
        Hand = new();
        Discard = new();

        // all cards
        AllCards = new();
        foreach (var card in Deck.Cards)
            AllCards.Add(card, Zones.DECK);

        _match.SystemLogger.Log("PLAYER", "Added player " + name);
    }

    /// <summary>
    /// Forces the player to draw an amount of cards
    /// </summary>
    /// <param name="amount">Amount of cards</param>
    public int Draw(int amount) {
        var cards = Deck.PopTop(amount);

        foreach (var card in cards)
            AllCards[card] = Zones.HAND;

        // _match.Emit("card_draw", new(){{"player", ToLuaTable(_match.LState)}, {"amount", amount}});

        Hand.AddToBack(cards);
        _match.SystemLogger.Log("PLAYER", "Player " + ShortStr + " drew " + cards.Count + " cards");

        return cards.Count;
    }

    public string ShortStr => Name + " [" + ID + "]";

    // public LuaTable ToSmallLuaTable(Lua lState) {
    //     var result = LuaUtility.CreateTable(lState);

    //     return result;
    // }

    public bool TryPlayCard(MCard card) {
        var canPlay = card.ExecCheckerFunc(MCard.CAN_PLAY_FNAME, card.Data, ID);
        if (!canPlay) {
            _match.SystemLogger.Log("WARN", "Player " + Name + " tried to play a card they can't play: " + card.ShortStr);
            return false;
        }

        var payed = card.ExecCheckerFunc(MCard.PAY_COSTS_FNAME, card.Data, ID);
        if (!payed) {
            _match.SystemLogger.Log("WARN", "Player " + Name + " decided not to cast card " + card.ShortStr);
            return false;
        }

        return true;
    }
}