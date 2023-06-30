using core.cards;
using core.decks;
using core.match;

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


    // zones
    public Zone<MCard> Deck { get; }
    public Zone<MCard> Hand { get; }


    public Player(Match match, string name, DeckTemplate dTemplate, PlayerController controller) {
        _match = match;
        Name = name;
        Controller = controller;

        match.Players.Add(this);
        ID = match.PlayerIDCreator.Next();

        // zones
        Deck = dTemplate.ToDeck(match);
        Hand = new();

        _match.SystemLogger.Log("PLAYER", "Added player " + name);
    }

    /// <summary>
    /// Forces the player to draw an amount of cards
    /// </summary>
    /// <param name="amount">Amount of cards</param>
    public void Draw(int amount) {
        var cards = Deck.PopTop(amount);

        // _match.Emit("card_draw", new(){{"player", ToLuaTable(_match.LState)}, {"amount", amount}});

        Hand.AddToBack(cards);
        _match.SystemLogger.Log("PLAYER", "Player " + ShortStr + " drew " + cards.Count + " cards");
    }

    public string ShortStr => Name + " [" + ID + "]";
}