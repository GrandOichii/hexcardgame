using Godot;
using System;

namespace HexClient.Match.Player;

public interface IPlayerInfo {
	public void LoadState(BaseState state);
}

// TODO add more descriptors
public partial class PlayerInfo : Control, IPlayerInfo
{
	#region Nodes

	public Label NameLabelNode { get; private set; }
	public Label EnergyLabelNode { get; private set; }
	public PanelContainer BgNode { get; private set; }

	#endregion

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
		var pState = state.Players[PlayerI];

		NameLabelNode.Text = pState.Name;
		EnergyLabelNode.Text = ToEnergyText(pState.Energy);
		BgColor = state.CurPlayerID == pState.ID ? CurrentPlayerColor : _defaultBgColor;
	}

}
