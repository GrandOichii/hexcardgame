using Godot;
using System;

namespace HexClient.Match;

public partial class Match : Control
{
	#region Node
	
	public Window OptionsWindowNode { get; private set; }
	
	#endregion

	public string MatchId {
		set {
			// CallDeferred("SetOptionsWindowTitle", $"Match {value} options");
		}
	}

	public override void _Ready()
	{
		#region Node fetching
		
		OptionsWindowNode = GetNode<Window>("%OptionsWindow");
		
		#endregion
		
		OptionsWindowNode.Hide();
	}

	private void SetOptionsWindowTitle(string title) {
		OptionsWindowNode.Title = title;
	}

	#region Signal connections
	


	private void OnShowCardIdsToggleToggled(bool buttonPressed)
	{
		// TODO
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
	
	#endregion

}




