using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;
using HexCore.GameMatch;

namespace HexClient.Match.State;

public class BaseState {
	public List<List<MatchLogEntryPart>> NewLogs { get; set; }
	public string Request { get; set; }
	public List<HexStates.PlayerState> Players { get; set; }
	public HexStates.MapState Map { get; set; }
	public string CurPlayerID { get; set; }
	public List<string> Args { get; set; }

	public virtual void ApplyTo(Match match, HexStates.MatchInfoState info) {
		// TODO

		// apply player order fix
		var container = match.PlayerContainerNode;
		if (info.MyI is not null && (container.GetChild(0) as PlayerInfo).PlayerI != info.MyI) {
			GD.Print("applying fixes to player containers");
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
