using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;
using HexCore.GameMatch;

namespace HexClient.Match.State;

public partial class BaseState : Node {
	public List<List<MatchLogEntryPart>> NewLogs { get; set; }
	public string Request { get; set; }
	public List<HexStates.PlayerState> Players { get; set; }
	public HexStates.MapState Map { get; set; }
	public string CurPlayerID { get; set; }
	public List<string> Args { get; set; }

	public virtual void ApplyTo(Match match, HexStates.MatchInfoState info) {
		CallDeferred("ApplyToPlayerInfo", match, new Wrapper<HexStates.MatchInfoState>(info));
//		ApplyToPlayerInfo(match, info);
	}

	public void ApplyToPlayerInfo(Match match, Wrapper<HexStates.MatchInfoState> infoW) {
		var info = infoW.Value;
		
		// apply player order fix
		var container = match.PlayerContainerNode;
		if (info.MyI is not null && (container.GetChild(info.PlayerCount - 1) as PlayerInfo).PlayerI != info.MyI) {
			var myI = info.MyI ?? default;
			var pCount = info.PlayerCount;
			for (int i = 0; i < pCount; i++) {
				var pNode = container.GetChild(i) as PlayerInfo;
				pNode.PlayerI = (i + myI + 1) % pCount;
			}
		}

		// update players
		foreach (var child in container.GetChildren()) {
			switch (child) {
			case PlayerInfo pInfo:
				pInfo.LoadState(this);
				break;
			default:
				break;
			}
		}
	}
}
