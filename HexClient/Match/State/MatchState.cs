using System.Collections.Generic;
using HexCore.GameMatch;

namespace HexClient.Match.State;

// !FIXME sometimes when starting match doesn't show the match config, requires to send an action to show the actual match

public partial class MatchState : BaseState {
	public HexStates.MyDataState MyData { get; set; }

	public override void ApplyTo(Match match, HexStates.MatchInfoState info)
	{
		base.ApplyTo(match, info);

		// hand cards
		var cCount = match.HandContainerNode.GetChildCount();
		var nCount = MyData.Hand.Count;

		if (nCount > cCount) {
			// fill hand up to new count
			for (int i = 0; i < nCount - cCount; i++) {
				var child = match.HandCardPS.Instantiate();
				match.HandContainerNode.AddChild(child);

				var cd = child as IHandCard;
				cd.SetCommandProcessor(match.Processor);
				cd.SetShowCardIds(match.ShowCardIds);
				match.ShowCardIdsToggled += cd.SetShowCardIds;
			}
		}
		if (nCount < cCount) {
			// trim child count
			for (int i = cCount - 1; i >= nCount; i--) {
			// for (int i = nCount + 1; i < cCount; i++) {
				var child = match.HandContainerNode.GetChild(i);
				child.Free();
			}
		}
		// load card data
		for (int i = 0; i < nCount; i++) {
			(match.HandContainerNode.GetChild(i) as IHandCard).Load(MyData.Hand[i]);
		}
	}
}
