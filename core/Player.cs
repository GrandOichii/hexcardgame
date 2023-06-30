using core.decks;
using core.match;

namespace core.players;


abstract public class PlayerController {

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
    }
}