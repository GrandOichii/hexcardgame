using Godot;
using HexClient.Match.Grid;
using HexCore.GameMatch.States;
using System;
using System.Collections.Generic;

namespace HexClient.Match;

public interface IPlayerDisplay {
	public void SetPlayerI(int playerI);
	public void OnShowPlayerIdsToggled(bool v);
}

public interface IMapGrid {
	public void Load(BaseState state);
	public void SetPlayerColors(Dictionary<string, Color> colors);
	public void SetCommandProcessor(CommandProcessor processor);
	public void SetHoverCard(IHoverCard card);
}

public interface IHandCard : IGamePart {
	public void Load(MatchCardState state);
	public void SetShowCardIds(bool v);
	public MatchCardState GetState();
	public void SetCommandProcessor(CommandProcessor processor);
}

public interface IHoverCard {
	public void Load(MatchCardState state);
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
	public ColorPickerButton Player1ColorPickerNode { get; private set; }
	public ColorPickerButton Player2ColorPickerNode { get; private set; }
	public IHoverCard HoverCardNode { get; private set; }
	
	public Window OptionsWindowNode { get; private set; }
	
	#endregion

	public string MatchId {
		set {
		}
	}

	public bool ShowCardIds { get; private set; }
	private Dictionary<string, Color> _playerColors = new();

	#nullable enable
	public CommandProcessor? Processor { get; private set; }
	#nullable disable

	public override void _Ready()
	{
		#region Node fetching
		
		OptionsWindowNode = GetNode<Window>("%OptionsWindow");
		PlayerContainerNode = GetNode<Control>("%PlayerContainer");
		HandContainerNode = GetNode<Container>("%HandContainer");
		LogsNode = GetNode<RichTextLabel>("%Logs");
		MapGridNode = GetNode<IMapGrid>("%MapGrid");
		Player1ColorPickerNode = GetNode<ColorPickerButton>("%Player1ColorPicker");
		Player2ColorPickerNode = GetNode<ColorPickerButton>("%Player2ColorPicker");
		HoverCardNode = GetNode<IHoverCard>("%HoverCard");
		
		#endregion

		MapGridNode.SetHoverCard(HoverCardNode);
		
		OnPlayer1ColorPickerColorChanged(Player1ColorPickerNode.Color);
		OnPlayer2ColorPickerColorChanged(Player2ColorPickerNode.Color);
		
		OptionsWindowNode.Hide();
	}
	
	public void SetCommandProcessor(CommandProcessor processor) {
		Processor = processor;

		MapGridNode.SetCommandProcessor(Processor);
	}

	private void SetOptionsWindowTitle(string title) {
		OptionsWindowNode.Title = title;
	}

	public void LoadMatchInfo(MatchInfoState info) {
		if (Processor is not null)
			Processor.Config = info;

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

	private void OnPlayer2ColorPickerColorChanged(Color color)
	{
		_playerColors["2"] = color;
		MapGridNode.SetPlayerColors(_playerColors);
	}

	private void OnPlayer1ColorPickerColorChanged(Color color)
	{
		_playerColors["1"] = color;
		MapGridNode.SetPlayerColors(_playerColors);
	}
	
	#endregion
}

