using Godot;
using HexCore.GameMatch.States;
using System;

namespace HexClient.Match.Player;

public interface IPlayerInfo {
	public void Load(PlayerState state);
}

// TODO add more descriptors
public partial class PlayerInfo : Control, IPlayerInfo
{
	#region Nodes

	public Label NameLabelNode { get; private set; }
	public Label EnergyLabelNode { get; private set; }

	#endregion

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching

		NameLabelNode = GetNode<Label>("%NameLabel");
		EnergyLabelNode = GetNode<Label>("%EnergyLabel");

		#endregion
	}
	
	private string ToEnergyText(int energy) => $"Energy: {energy}";

	public void Load(PlayerState state)
	{
		NameLabelNode.Text = state.Name;
		EnergyLabelNode.Text = ToEnergyText(state.Energy);
	}

}
