namespace HexCore.GameMatch.Players.Controllers;


/// <summary>
/// Controller class, handles actions of player
/// </summary>
public interface IPlayerController {

    /// <summary>
    /// Prompts action and records it
    /// </summary>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    /// <returns>The prompted action</returns>
    public Task<string> DoPromptAction(Player player, Match match);

    /// <summary>
    /// Setups the player for the match
    /// </summary>
    /// <param name="player">Controller player</param>
    /// <param name="match">Match</param>
    public Task Setup(Player player, Match match);

    /// <summary>
    /// Updates the player abount the match
    /// </summary>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    public Task Update(Player player, Match match);

    /// <summary>
    /// Cleans up the player data
    /// </summary>
    public Task CleanUp();

    /// <summary>
    /// Prompts the controlled player to pick a tile
    /// </summary>
    /// <param name="choices">List of tile coordinates. If empty, considers all tile as valid choices</param>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    /// <returns></returns>
    public Task<string> DoPickTile(List<int[]> choices, Player player, Match match);

    /// <summary>
    /// Sends the card info to the player
    /// </summary>
    /// <param name="card"></param>
    public Task SendCard(Match match, Player player, ExpansionCard card);

    /// <summary>
    /// Forces the controlled player to pick a tile and records it
    /// </summary>
    /// <param name="choices">List of tile coordinates. If empty, considers all tiles as valid choices</param>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    /// <returns></returns>
    public async Task<string> PickTile(List<int[]> choices, Player player, Match match) {
        var result = await DoPickTile(choices, player, match);
        // TODO record
        // TODO? check for correctness
        return result;
    }

    /// <summary>
    /// Prompts the action from player
    /// </summary>
    /// <param name="player">Controller player</param>
    /// <param name="match">Match</param>
    /// <returns>Action of the player</returns>
    public async Task<string> PromptAction(Player player, Match match) {
        var result = await DoPromptAction(player, match);
        // TODO
        return result;
    }
}
