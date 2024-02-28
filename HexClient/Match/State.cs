using System.Collections.Generic;
using HexCore.GameMatch;
using HexCore.GameMatch.States;

namespace HexClient.Match.State;

public class BaseState {
    public List<List<MatchLogEntryPart>> NewLogs { get; set; }
    public string Request { get; set; }
    public List<PlayerState> Players { get; set; }
    public MapState Map { get; set; }
    public string CurPlayerID { get; set; }
    public List<string> Args { get; set; }
}

public class MatchState : BaseState {
    public MyDataState MyData { get; set; }
}