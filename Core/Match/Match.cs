using NLua;
using Util;

namespace Core.GameMatch;

/// <summary>
/// Match object, is used for playing matches
/// </summary>
public class Match
{
    public Random Rnd { get; }
    private static readonly List<IMatchPhase> _phases = new(){
        new TurnStart(),
        new MainPhase(),
        new TurnEnd()
    };

    public IIdCreator PlayerIDCreator { get; set; } = new BasicIDCreator();
    public IIdCreator CardIDCreator { get; set; } = new BasicIDCreator();
    public ICardMaster CardMaster { get; }
    public string ID { get; }
    public bool StrictMode { get; set; } = true;
    public List<Player> Players { get; }
    public Lua LState { get; }
    public ScriptMaster ScriptMaster { get; }
    public Map Map { get; }
    public Player? Winner { get; set; } = null;
    public bool Active => Winner is null;
    public bool AllowCommands { get; set; }
    public ILogger SystemLogger { get; set; } = new EmptyLogger();
    public IMatchView View { get; set; } = new EmptyMatchView();
    public MatchConfig Config { get; }
    public MatchLogger Logger { get; }


    // TODO remove
    public readonly int MaxPass = 50;
    public int PassCount { get; set; } = 0;
    public Match(string id, MatchConfig config, ICardMaster master, string coreFilePath) {
        CardMaster = master;
        Logger = new(this);
        ID = id;
        LState = new();
        ScriptMaster = new(this);
        Players = new();
        Config = config;
        Rnd = new Random(config.Seed);

        Map = Map.FromConfig(this, config);

        SystemLogger.Log("MATCH", "Running core file");
        LState.DoFile(coreFilePath);

        SystemLogger.Log("MATCH", "Running addons");
        foreach (var addonPath in config.AddonPaths) {
            LState.DoFile(addonPath);
            var f = LState.GetFunction("_Apply");
            f.Call();
        }
    }

    /// <summary>
    /// Checks whether can add a player with the specified name and deck
    /// </summary>
    /// <param name="name">Name of the player to be added</param>
    /// <param name="deck">Deck of the player to be added</param>
    /// <returns>A string with the reason why the player can't be added, otherwise returns empty string</returns>
    public string CanAddPlayer(string name, DeckTemplate deck) {
        foreach (var player in Players)
            if (player.Name == name)
                return "Player with name " + name + " is already joined.";
        // TODO check deck
        return "";
    }

    /// <summary>
    /// Starts the match
    /// </summary>
    public void Start() {
        Setup();
        Turns();
        CleanUp();
    }

    /// <summary>
    /// Sets up the match
    /// </summary>
    public void Setup() {
        SystemLogger.Log("MATCH", "Setting up match");
        
        SystemLogger.Log("MATCH", "Running map setup script");
        LState.DoString(Config.SetupScript);

        // setup players
        foreach (var player in Players) {
            player.Setup();
        }

        View.Start();
        View.Update(this);
    }

    /// <summary>
    /// The main loop of the match
    /// </summary>
    public void Turns() {
        SystemLogger.Log("MATCH", "Started main match loop");

        Logger.ParseAndLog("Match started");
        while (Winner is null) {
            View.Update(this);
            var cPlayer = CurrentPlayer;
            Logger.ParseAndLog(cPlayer.Name + " started their turn.");

            foreach (var phase in _phases) {
                phase.Exec(this, cPlayer);

                if (!Active) break;
            }
            Logger.ParseAndLog("Player " + cPlayer.Name + " passed their turn.");
            
            ++CurPlayerI;
        }
    }

    /// <summary>
    /// Cleans up after the match is finished
    /// </summary>
    public void CleanUp() {
        SystemLogger.Log("MATCH", "Performing cleanup");

        foreach (var player in Players) {
            player.Controller.CleanUp();
        }
        View.End();
    }

    /// <summary>
    /// Returns the current player
    /// </summary>
    /// <value>Current player</value>
    public Player CurrentPlayer => Players[CurPlayerI];
    private int _curPlayerI;
    public int CurPlayerI
    {
        get => _curPlayerI;
        set { 
            _curPlayerI = value; 
            if (_curPlayerI >= Players.Count)
                _curPlayerI = 0;
            if (_curPlayerI < 0)
                _curPlayerI = Players.Count - 1;
        }
    }
    
    /// <summary>
    /// Returns a player with the given ID
    /// </summary>
    /// <param name="pID">Player ID</param>
    /// <returns>Player</returns>
    public Player PlayerWithID(string pID) {
        foreach (var player in Players)
            if (player.ID == pID)
                return player;
        
        throw new Exception("Failed to find a player with ID " + pID);
    }

    /// <summary>
    /// Emits a singal to all cards
    /// </summary>
    /// <param name="signal">Signal string</param>
    /// <param name="args">Signal arguments</param>
    public void Emit(string signal, Dictionary<string, object> args) {
        var logMessage = "Emitted signal " + signal + ", args: ";
        foreach (var pair in args) logMessage += pair.Key + ":" + pair.Value.ToString() + " ";
        SystemLogger.Log("MATCH", logMessage);

        foreach (var player in Players) {
            var cards = player.AllCards;
            foreach (var pair in cards) {
                var zone = pair.Value;
                var card = pair.Key;
                var triggers = LuaUtility.TableGet<LuaTable>(card.Data, "triggers");
                foreach (var triggerO in triggers.Values) {
                    var triggerRaw = triggerO as LuaTable ?? throw new Exception("Trigger of card " + card.ShortStr + " is somehow value " + triggerO + " (not LuaTable)");
                    var z = LuaUtility.TableGet<string>(triggerRaw, "zone");
                    if (z != zone) continue;
                    var on = LuaUtility.TableGet<string>(triggerRaw, "on");
                    if (on != signal) continue;

                    var trigger = new Trigger(triggerRaw);
                    SystemLogger.Log("MATCH", "Card " + card.ShortStr + " in zone " + zone + " of player " + player.ShortStr + " has a potential trigger");

                    var triggered = trigger.ExecCheck(LState, player, args);
                    if (!triggered) {
                        SystemLogger.Log("MATCH", "Card " + card.ShortStr + " in zone " + zone + " of player " + player.ShortStr + " failed to trigger");
                        continue;
                    }

                    var payed = trigger.ExecCosts(LState, player, args);
                    if (!payed) {
                        SystemLogger.Log("MATCH", "Player " + player.ShortStr + " did not pay cost of triggered ability of card " + card.ShortStr + " in zone " + zone);
                        continue;
                    }

                    SystemLogger.Log("MATCH", "Card " + card.ShortStr + " in zone " + zone + " of player " + player.ShortStr + " triggers");
                    trigger.ExecEffect(LState, player, args);
                }
            }
        }

        SystemLogger.Log("MATCH", "Finished emitting " + signal);
    }

    /// <summary>
    /// Checks all Units and Structures for having 0 life
    /// </summary>
    public void CheckZeroLife() {
        for (int i = 0; i < Map.Height; i++) {
            for (int j = 0; j < Map.Width; j++) {
                var tile = Map.Tiles[i, j];
                if (tile is null) continue;
                var en = tile.Entity;
                if (en is null) continue;
                if (!en.IsPlaceable) continue;
                if (en.Life > 0) continue;
                if (en.Name == "Castle") {
                    var loserID = en.Owner.ID;
                    foreach (var player in Players) {
                        if (player.ID != loserID) {
                            Winner = player;
                            return;
                        }
                    }
                }
                
                en.Owner.AllCards[en] = "";
                
                tile.Entity = null;
                if (!en.GoesToDiscard) continue;
                
                en.Owner.Discard.AddToBack(en);
                SystemLogger.Log("MATCH", "Placing card " + en.ShortStr + " into discard");
                en.Owner.AllCards[en] = ZoneTypes.DISCARD;

                if (en.IsUnit) tile.HasGrave = true;
            }
        }
    }

    /// <summary>
    /// Returns the card with the specified match ID
    /// </summary>
    /// <param name="mID">Card match ID</param>
    /// <returns>Match card</returns>
    public MCard GetCard(string mID) {
        foreach (var player in Players)
            foreach (var card in player.AllCards.Keys)
                if (card.MID == mID)
                    return card;

        throw new Exception("Failed to get owner of the card with match ID " + mID);
    }

    /// <summary>
    /// Updates all of the current player's opponents
    /// </summary>
    public void UpdateOpponents() {
        var cur = CurrentPlayer;
        foreach (var player in Players)
            if (player != cur)
                player.Controller.Update(player, this);
    }
}


