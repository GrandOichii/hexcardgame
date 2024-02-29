using System.Collections.Generic;
using HexCore.GameMatch;

namespace HexClient.Match.State;

public class MatchState : BaseState {
	public HexStates.MyDataState MyData { get; set; }

	public override void ApplyTo(Match match, HexStates.MatchInfoState info)
	{
		base.ApplyTo(match, info);

		// TODO
	}
}
