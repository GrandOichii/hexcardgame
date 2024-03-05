using Godot;
using System;

namespace HexClient.Match;

public partial class Match : Control
{
	#region Packed scenes

	[Export]
	private PackedScene PlayerInfoPS { get; set; }

	#endregion

	#region Nodes
	
	public Window OptionsWindowNode { get; private set; }
	public Control PlayerContainerNode { get; private set; }
	
	#endregion

	public string MatchId {
		set {
			// CallDeferred("SetOptionsWindowTitle", $"Match {value} options");
		}
	}

	public override void _Ready()
	{
		#region Node fetching
		
		OptionsWindowNode = GetNode<Window>("%OptionsWindow");
		PlayerContainerNode = GetNode<Control>("%PlayerContainer");
		
		#endregion
		
		OptionsWindowNode.Hide();
	}

	private void SetOptionsWindowTitle(string title) {
		OptionsWindowNode.Title = title;
	}

	public void LoadMatchInfo(HexStates.MatchInfoState info) {
		
		// create player info nodes
		var pCount = info.PlayerCount;
		for (int i = 0; i < pCount; i++) {
			CallDeferred("CreatePlayerInfo", i);
		}
	}

	private void CreatePlayerInfo(int playerI) {
		var child = PlayerInfoPS.Instantiate() as PlayerInfo;
		PlayerContainerNode.AddChild(child);
		// PlayerContainerNode.AddChild(child);
		// child.Client = Client;
		child.PlayerI = playerI;
	}

	#region Signal connections

	private void OnShowCardIdsToggleToggled(bool buttonPressed)
	{
		// TODO
	}

	private void OnShowOptionsButtonPressed()
	{
		OptionsWindowNode.Show();
		OptionsWindowNode.GrabFocus();
	}

	private void OnOptionsWindowCloseRequested()
	{
		OptionsWindowNode.Hide();
	}
	
	#endregion

}




