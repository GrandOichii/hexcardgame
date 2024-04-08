using System.Collections.Generic;
using HexCore.GameMatch;

namespace HexClient.Match.State;

public partial class MatchState : BaseState {
	public HexStates.MyDataState MyData { get; set; }

	public void ApplyTo(ConnectedMatch match, HexStates.MatchInfoState info) {
		base.ApplyTo(match.MatchNode, info);

		match.HandContainerNode.Load(this, match.MatchNode, match.HandCardPS);
		// hand cards
	}

	public override void ApplyTo(Match match, HexStates.MatchInfoState info)
	{
		base.ApplyTo(match, info);
	}
}
