

namespace ManagerBack;

public class RecordingPlayerController : IPlayerController
{
    private readonly IPlayerController _baseController;
    private readonly PlayerRecord _record;

    public RecordingPlayerController(IPlayerController baseController, PlayerRecord record)
    {
        _baseController = baseController;
        _record = record;
    }

    public async Task CleanUp()
    {
        await _baseController.CleanUp();
    }

    public async Task<string> DoPickTile(List<int[]> choices, Player player, Match match)
    {
        var result = await _baseController.DoPickTile(choices, player, match);
        await Record(result);
        return result;
    }

    public async Task<string> DoPromptAction(Player player, Match match)
    {
        var result = await _baseController.DoPromptAction(player, match);
        await Record(result);
        return result;
    }

    public async Task SendCard(Match match, Player player, ExpansionCard card)
    {
        await _baseController.SendCard(match, player, card);

    }

    public async Task Setup(Player player, Match match)
    {
        System.Console.WriteLine("Sending setup to " + player.Name);
        await _baseController.Setup(player, match);
    }

    public async Task Update(Player player, Match match)
    {
        await _baseController.Update(player, match);
    }

    private Task Record(string action) {
        _record.Actions.Add(action);
        return Task.CompletedTask;
    }

}