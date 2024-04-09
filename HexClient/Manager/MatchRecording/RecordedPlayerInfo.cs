using Godot;
using HexCore.GameMatch.States;
using System;

namespace HexClient.Manager.Recording;

public partial class RecordedPlayerInfo : Control, IRecordedPlayerInfo
{
	#region Nodes

	public PlayerInfo PlayerInfoNode { get; private set; }
	public HandContainer HandContainerNode { get; private set; }

	#endregion

	public override void _Ready()
	{
		#region Node fetching
		
		PlayerInfoNode = GetNode<PlayerInfo>("%PlayerInfo");
		HandContainerNode = GetNode<HandContainer>("%HandContainer");

		
		#endregion
	}

	public void SetPlayerI(int playerI) {
		PlayerInfoNode.PlayerI = playerI;
	} 

	public void Load(MyDataState pState, BaseState state, Match.Match match, PackedScene handCardPS)
	{
		PlayerInfoNode.LoadState(state);
		
		HandContainerNode.Load(pState, match, handCardPS);
	}
}
