namespace Core.GameMatch.Players.Controllers;


/// <summary>
/// Controller class, handles actions of player
/// </summary>
public interface IPlayerController {
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

    /// <summary>
    /// Prompts action and records it
    /// </summary>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    /// <returns>The prompted action</returns>
    public string DoPromptAction(Player player, Match match);

    /// <summary>
    /// Setups the player for the match
    /// </summary>
    /// <param name="player">Controller player</param>
    /// <param name="match">Match</param>
    public void Setup(Player player, Match match);

    /// <summary>
    /// Updates the player abount the match
    /// </summary>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    public void Update(Player player, Match match);

    /// <summary>
    /// Cleans up the player data
    /// </summary>
    public void CleanUp();

    /// <summary>
    /// Forces the controlled player to pick a tile and records it
    /// </summary>
    /// <param name="choices">List of tile coordinates. If empty, considers all tiles as valid choices</param>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    /// <returns></returns>
    public string PickTile(List<int[]> choices, Player player, Match match) {
        var result = DoPickTile(choices, player, match);
        // TODO record
        // TODO? check for correctness
        return result;
    }

    /// <summary>
    /// Prompts the controlled player to pick a tile
    /// </summary>
    /// <param name="choices">List of tile coordinates. If empty, considers all tile as valid choices</param>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    /// <returns></returns>
    public string DoPickTile(List<int[]> choices, Player player, Match match);

    /// <summary>
    /// Sends the card info to the player
    /// </summary>
    /// <param name="card"></param>
    public void SendCard(Match match, Player player, ExpansionCard card);
}
