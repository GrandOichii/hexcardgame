using Godot;
using System;
using System.IO;

namespace HexClient.Manager;

public partial class PlayerConfig : Control
{
	#region Nodes
	
	public CheckButton IsBotCheckNode { get; private set; }
	public Control BotConfigNode { get; private set; }
	public LineEdit BotNameEditNode { get; private set; }
	public LineEdit DeckPathEditNode { get; private set; }
	public FileDialog ChooseDeckFileDialogNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		IsBotCheckNode = GetNode<CheckButton>("%IsBotCheck");
		BotConfigNode = GetNode<Control>("%BotConfig");
		BotNameEditNode = GetNode<LineEdit>("%BotNameEdit");
		DeckPathEditNode = GetNode<LineEdit>("%DeckPathEdit");
		ChooseDeckFileDialogNode = GetNode<FileDialog>("%ChooseDeckFileDialog");
		
		#endregion

		OnIsBotCheckToggled(IsBotCheckNode.ButtonPressed);
	}
	
	public MatchPlayerConfig Baked {
		// TODO validate deck
		// TODO add bot type choosing

		get => new()
		{
			BotConfig = IsBotCheckNode.ButtonPressed ? new() {
				Name = BotNameEditNode.Text,
				StrDeck = File.ReadAllText(DeckPathEditNode.Text),
				BotType = HexClient.Manager.BotType.RANDOM
			} : null
		};
	}

	#region Signal connections

	private void OnIsBotCheckToggled(bool buttonPressed)
	{
		BotConfigNode.Visible = buttonPressed;
	}

	private void OnChooseDeckButtonPressed()
	{
		ChooseDeckFileDialogNode.Show();
	}

	private void OnChooseDeckFileDialogFileSelected(string path)
	{
		DeckPathEditNode.Text = path;
	}
	
	#endregion
}



