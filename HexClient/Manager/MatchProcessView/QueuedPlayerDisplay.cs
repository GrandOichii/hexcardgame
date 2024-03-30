using Godot;
using System;

namespace HexClient.Manager;

public partial class QueuedPlayerDisplay : Control, IQueuedPlayerDisplay
{
	#region Nodes

	public Container NameContainerNode { get; private set; }
	public Label NameLabel { get; private set; }
	public Control BotIndicatorNode { get; private set; }
	public Container DeckContainerNode { get; private set; }
	public Button RevealDeckButtonNode { get; private set; }
	public LineEdit DeckDisplayNode { get; private set; }
	public Label StatusLabelNode { get; private set; }

	#endregion


	public override void _Ready()
	{
		#region Node fetching

		NameContainerNode = GetNode<Container>("%NameContainer");
		NameLabel = GetNode<Label>("%NameLabel");
		BotIndicatorNode = GetNode<Control>("%BotIndicator");
		DeckContainerNode = GetNode<Container>("%DeckContainer");
		RevealDeckButtonNode = GetNode<Button>("%RevealDeckButton");
		DeckDisplayNode = GetNode<LineEdit>("%DeckDisplay");
		StatusLabelNode = GetNode<Label>("%StatusLabel");

		#endregion
	}

	#nullable enable
	public void Load(QueuedPlayer? player)
	#nullable disable
	{
		NameContainerNode.Show();
		DeckContainerNode.Show();

		if (player is null) {
			StatusLabelNode.Text = "Waiting for connection";
			NameContainerNode.Hide();
			DeckContainerNode.Hide();
			return;
		}
		
		NameLabel.Text = player.Name;
		StatusLabelNode.Text = player.Status.ToFriendlyString();
		DeckDisplayNode.Hide();
		RevealDeckButtonNode.Show();
		DeckDisplayNode.Text = player.Deck;
		BotIndicatorNode.Visible = player.IsBot;
	}

	#region Signal connections
	
	private void OnRevealDeckButtonPressed()
	{
		DeckDisplayNode.Show();
		RevealDeckButtonNode.Hide();
	}
	
	#endregion
}
