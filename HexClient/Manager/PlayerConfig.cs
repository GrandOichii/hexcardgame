using Godot;
using System;
using System.IO;
using HexCore.Decks;

namespace HexClient.Manager;

public partial class PlayerConfig : Control
{
	#region Nodes
	
	public CheckButton IsBotCheckNode { get; private set; }
	public Control BotConfigNode { get; private set; }
	public LineEdit BotNameEditNode { get; private set; }
	public LineEdit DeckPathEditNode { get; private set; }
	public FileDialog ChooseDeckFileDialogNode { get; private set; }
	public OptionButton BotTypeOptionNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		IsBotCheckNode = GetNode<CheckButton>("%IsBotCheck");
		BotConfigNode = GetNode<Control>("%BotConfig");
		BotNameEditNode = GetNode<LineEdit>("%BotNameEdit");
		DeckPathEditNode = GetNode<LineEdit>("%DeckPathEdit");
		ChooseDeckFileDialogNode = GetNode<FileDialog>("%ChooseDeckFileDialog");
		BotTypeOptionNode = GetNode<OptionButton>("%BotTypeOption");
		
		#endregion

		foreach (BotType botType in Enum.GetValues(typeof(BotType))) {
			// TODO make more user friendly
			BotTypeOptionNode.AddItem(botType.ToFriendlyString());
			BotTypeOptionNode.SetItemMetadata(BotTypeOptionNode.ItemCount - 1, new Wrapper<BotType>(botType));
		}

		BotTypeOptionNode.Select(0);

		OnIsBotCheckToggled(IsBotCheckNode.ButtonPressed);
	}
	
	public MatchPlayerConfig Baked {
		get {
			var data = File.ReadAllText(DeckPathEditNode.Text);
			_ = DeckTemplate.FromText(data);

			return new()
			{
				BotConfig = IsBotCheckNode.ButtonPressed ? new() {
					Name = BotNameEditNode.Text,
					StrDeck = data,
					BotType = BotTypeOptionNode.GetSelectedMetadata().As<Wrapper<BotType>>().Value
				} : null
			};
		}
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



