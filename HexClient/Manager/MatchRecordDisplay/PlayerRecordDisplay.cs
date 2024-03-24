using Godot;
using System;

namespace HexClient;

public partial class PlayerRecordDisplay : Control, IPlayerRecordDisplay
{
	#region Nodes
	
	public Label NameLabelNode { get; private set; }
	public ItemList ActionListNode { get; private set; }

	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		NameLabelNode = GetNode<Label>("%NameLabel");
		ActionListNode = GetNode<ItemList>("%ActionList");
		
		#endregion
	}

	public void Load(PlayerRecord record) {
		NameLabelNode.Text = $"Name: {record.Name}";

		foreach (var action in record.Actions) {
			var i = ActionListNode.AddItem(action);
		}
	}
}
