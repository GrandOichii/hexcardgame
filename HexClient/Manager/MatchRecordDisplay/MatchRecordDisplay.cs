using Godot;
using System;

namespace HexClient;

public interface IPlayerRecordDisplay {
	public void Load(PlayerRecord record);
}

public partial class MatchRecordDisplay : Control
{
	#region Packed scenes

	[Export]
	private PackedScene PlayerRecordDisplayPS { get; set; }

	#endregion

	#region Nodes
	
	public Button ShowExceptionButtonNode { get; private set; }
	public Button ShowInnerExceptionButtonNode { get; private set; }
	public Label WinnerNameLabelNode { get; private set; }
	public Container PlayerRecordContainerNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		ShowExceptionButtonNode = GetNode<Button>("%ShowExceptionButton");
		ShowInnerExceptionButtonNode = GetNode<Button>("%ShowInnerExceptionButton");
		WinnerNameLabelNode = GetNode<Label>("%WinnerNameLabel");
		PlayerRecordContainerNode = GetNode<Container>("%PlayerRecordContainer");
		
		#endregion

		Load(new() {
			ExceptionMessage = "exception message here",
			WinnerName = "Jack",
			Players = new() {
				new() {
					Name = "Alice",
					Actions = new() { "pass", "action1", "action2" }
				},
				new() {
					Name = "Jack",
					Actions = new() { "pass", "win" }
				},
			},
		});
	}

	public void Load(MatchRecord record) {
		while (PlayerRecordContainerNode.GetChildCount() > 0)
			PlayerRecordContainerNode.RemoveChild(PlayerRecordContainerNode.GetChild(0));

		WinnerNameLabelNode.Text = $"Winner: {record.WinnerName}";

		ShowExceptionButtonNode.Visible = !string.IsNullOrEmpty(record.ExceptionMessage);
		ShowInnerExceptionButtonNode.Visible = !string.IsNullOrEmpty(record.InnerExceptionMessage);

		for (int i = 0; i < record.Players.Count; i++) {
			if (i != 0) PlayerRecordContainerNode.AddChild(new VSeparator());

			var child = PlayerRecordDisplayPS.Instantiate() as Control;
			child.SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand;
			PlayerRecordContainerNode.AddChild(child);

			var display = child as IPlayerRecordDisplay;
			display.Load(record.Players[i]);
		}
	}
	
	#region Signal connections
	
	
	
	#endregion
}
