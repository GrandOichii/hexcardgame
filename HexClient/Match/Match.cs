using Godot;
using HexClient.Match.Grid;
using System;

namespace HexClient.Match;

public interface IPlayerDisplay {
	public void SetPlayerI(int playerI);
	public void OnShowPlayerIdsToggled(bool v);
}

public interface IMapGrid {
	public void Load(BaseState state);
}

public partial class Match : Control
{
	#region Signals

	[Signal]
	public delegate void ShowCardIdsToggledEventHandler(bool v);
	[Signal]
	public delegate void ShowPlayerIdsToggledEventHandler(bool v);
	[Signal]
	public delegate void ShowTileIdsToggledEventHandler(bool b);
	[Signal]
	public delegate void ShowEntityIdsToggledEventHandler(bool b);

	#endregion

	#region Packed scenes

	[Export]
	private PackedScene PlayerInfoPS { get; set; }
	[Export]
	public PackedScene HandCardPS { get; set; }

	#endregion

	#region Nodes
	
	public Control PlayerContainerNode { get; private set; }
	public Container HandContainerNode { get; private set; }
	public RichTextLabel LogsNode { get; private set; }
	public IMapGrid MapGridNode { get; private set; }
	
	public Window OptionsWindowNode { get; private set; }
	
	#endregion

	public string MatchId {
		set {
		}
	}

	public bool ShowCardIds { get; private set; }

	public override void _Ready()
	{
		#region Node fetching
		
		OptionsWindowNode = GetNode<Window>("%OptionsWindow");
		PlayerContainerNode = GetNode<Control>("%PlayerContainer");
		HandContainerNode = GetNode<Container>("%HandContainer");
		LogsNode = GetNode<RichTextLabel>("%Logs");
		MapGridNode = GetNode<IMapGrid>("%MapGrid");
		
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
		var child = PlayerInfoPS.Instantiate();
		PlayerContainerNode.AddChild(child);

		var playerDisplay = child as IPlayerDisplay;
		playerDisplay.SetPlayerI(playerI);
		ShowPlayerIdsToggled += playerDisplay.OnShowPlayerIdsToggled;
	}

	#region Signal connections

	private void OnShowCardIdsToggleToggled(bool buttonPressed)
	{
		ShowCardIds = buttonPressed;
		EmitSignal(SignalName.ShowCardIdsToggled, buttonPressed);
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

	private void OnShowPlayerIdsToggleToggled(bool buttonPressed)
	{
		EmitSignal(SignalName.ShowPlayerIdsToggled, buttonPressed);
	}

	private void OnShowTileIdsToggleToggled(bool buttonPressed)
	{
		EmitSignal(SignalName.ShowTileIdsToggled, buttonPressed);
	}

	private void OnShowEntityIdsToggleToggled(bool buttonPressed)
	{
		EmitSignal(SignalName.ShowEntityIdsToggled, buttonPressed);
	}
	
	#endregion
}


