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

	#endregion

	public int PlayerI { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching

		NameLabelNode = GetNode<Label>("%NameLabel");
		EnergyLabelNode = GetNode<Label>("%EnergyLabel");

		#endregion
	}
	
	private static string ToEnergyText(int energy) => $"Energy: {energy}";

	public void LoadState(BaseState state)
	{
		var pState = state.Players[PlayerI];

		NameLabelNode.Text = pState.Name;
		EnergyLabelNode.Text = ToEnergyText(pState.Energy);
	}

}
