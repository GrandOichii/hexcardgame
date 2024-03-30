using Godot;
using System;

namespace HexClient.Manager;

public partial class MatchProcessViewWindow : Window
{
	#region Nodes

	public MatchProcessView MatchProcessViewNode { get; private set; }

	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching

		MatchProcessViewNode = GetNode<MatchProcessView>("%MatchProcessView");

		#endregion

		// TODO remove
		MatchProcessViewNode.Load("18c44bc1-8e10-43b6-916e-b70be65e61ab");
	}

	#region Signal connections

	private void OnCloseRequested()
	{
		QueueFree();
	}

	#endregion
}


