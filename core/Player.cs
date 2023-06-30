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
    public Match Match { get; }
    public string Name { get; }
    public PlayerController Controller { get; }
    public string ID { get; }

    public Player(Match match, string name, DeckTemplate dTemplate, PlayerController controller) {
        Match = match;
        Name = name;
        Controller = controller;

        match.Players.Add(this);
        ID = match.PlayerIDCreator.Next();

        // TODO decks

        Match.SystemLogger.Log("PLAYER", "Added player " + name);
    }
}