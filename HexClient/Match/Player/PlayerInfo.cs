using Godot;
using HexCore.GameMatch.States;
using System;

namespace HexClient.Match.Player;

public partial class PlayerInfo : Control, IPlayerDisplay
{
	#region Nodes

	public Label NameLabelNode { get; private set; }
	public Label EnergyLabelNode { get; private set; }
	public PanelContainer BgNode { get; private set; }

	#endregion

	private bool _showId = false;

	#region Exports

	[Export]
	public Color CurrentPlayerColor { get; set; }

	#endregion

	public int PlayerI { get; set; }

	private StyleBoxFlat _bgStyle => BgNode.Get("theme_override_styles/panel").As<StyleBoxFlat>();

	public Color BgColor {
		get => _bgStyle.BgColor;
		set {
			_bgStyle.BgColor = value;
		}
	}

	private Color _defaultBgColor;
	private PlayerState _state;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching

		BgNode = GetNode<PanelContainer>("%Bg");
		NameLabelNode = GetNode<Label>("%NameLabel");
		EnergyLabelNode = GetNode<Label>("%EnergyLabel");

		#endregion

		// * this prevents applying the same changes to all nodes with the same bg
		BgNode.Set("theme_override_styles/panel", _bgStyle.Duplicate());
		
		_defaultBgColor = BgColor;
	}
	
	private static string ToEnergyText(int energy) => $"Energy: {energy}";

	public void LoadState(BaseState state)
	{
		_state = state.Players[PlayerI];

		SetName(_state);
		EnergyLabelNode.Text = ToEnergyText(_state.Energy);
		BgColor = state.CurPlayerID == _state.ID ? CurrentPlayerColor : _defaultBgColor;
	}

	private void SetName(PlayerState state) {
		NameLabelNode.Text = state.Name;
		if (_showId)
			NameLabelNode.Text += " [" + state.ID + "]";
	}

	public void SetPlayerI(int playerI)
	{
		PlayerI = playerI;
	}

	public void OnShowPlayerIdsToggled(bool v)
	{
		_showId = v;
		SetName(_state);
	}
}
