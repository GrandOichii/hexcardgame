using System.Collections.Generic;
using HexCore.GameMatch;
using HexCore.GameMatch.States;

namespace HexClient.Match.State;

public class MatchState : BaseState {
    public MyDataState MyData { get; set; }

    public override void ApplyTo(Match match)
    {
        base.ApplyTo(match);

        // TODO
    }
}