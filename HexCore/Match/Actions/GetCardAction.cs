namespace HexCore.GameMatch.Actions;


/// <summary>
/// Action for getting card info
/// </summary>
class GetCardAction : IGameAction{
    public async Task Exec(Match match, Player player, string[] args)
    {
        if (args.Length < 2) {
            if (!match.StrictMode) return;
            throw new Exception("Incorrect number of arguments for get card action");
        }

        var cardID = "";
        for (int i = 1; i < args.Length; i++) {
            cardID += args[i];
            if (i == args.Length - 1) continue;

            cardID += " ";
        }
        var card = await match.CardMaster.Get(cardID);
        await player.Controller.SendCard(match, player, card);
    }
}

