using core.cards;

namespace ManagerBack.Models;

public class CardModel : ExpansionCard {
    public string? Id;

    public string GetCID() => CID;
}