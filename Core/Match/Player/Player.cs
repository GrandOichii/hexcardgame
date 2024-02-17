namespace Core.GameMatch.Players;

/// <summary>
/// In-match player object.
/// </summary>
public class Player {
    private readonly Match _match;
    // remove
    public string Name { get; set; }
    public IPlayerController Controller { get; }
    public string ID { get; }

    public int Energy { get; set; }
    private int _maxEnergy;
    public int MaxEnergy { get => _maxEnergy; set {
        _maxEnergy = value;
        var me =_match.Config.MaxEnergy;
        if (_maxEnergy > me && me != -1)
            _maxEnergy = me;
    } }

    public Dictionary<MCard, string> AllCards { get; }

    // zones
    public Zone<MCard> Deck { get; }
    public Zone<MCard> Discard { get; }
    public Zone<MCard> Hand { get; }

    // new log messages
    public List<List<MatchLogEntryPart>> NewLogs { get; set; }


    public Player(Match match, string name, DeckTemplate dTemplate, IPlayerController controller) {
        NewLogs = new();
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
            AllCards.Add(card, ZoneTypes.DECK);

        _match.SystemLogger.Log("PLAYER", "Added player " + name);
    }

    public void Setup() {
        Draw(_match.Config.StartingHandSize);

        Controller.Setup(this, _match);
    }

    /// <summary>
    /// Forces the player to draw an amount of cards
    /// </summary>
    /// <param name="amount">Amount of cards</param>
    public int Draw(int amount) {
        var cards = Deck.PopTop(amount);

        foreach (var card in cards)
            AllCards[card] = ZoneTypes.HAND;

        // _match.Emit("card_draw", new(){{"player", ToLuaTable(_match.LState)}, {"amount", amount}});

        Hand.AddToBack(cards);
        var message =  "Player " + ShortStr + " drew " + cards.Count + " card";
        if (cards.Count > 1)
            message += "s";
        _match.SystemLogger.Log("PLAYER", message + ".");

        message = Name + " drew " + cards.Count + " card";
        if (cards.Count > 1)
            message += "s";
        _match.Logger.ParseAndLog(message + ".");
        return cards.Count;
    }

    public string ShortStr => Name + " [" + ID + "]";

    /// <summary>
    /// Tries to check the requirements for playing the cards and tries to pay the costs of the card
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public bool TryPlayCard(MCard card) {
        var canPlay = card.CanBePlayed(this);
        if (!canPlay) return false;

        var payed = card.ExecCheckerFunc(MCard.PAY_COSTS_FNAME, card.Data, ID);
        if (!payed) {
            _match.SystemLogger.Log("WARN", "Player " + Name + " decided not to cast card " + card.ShortStr);
            return false;
        }

        return true;
    }
}