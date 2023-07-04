using Godot;
using System;

using Shared;

public partial class ActionSender : LineEdit
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void OnActionSenderTextSubmitted(string new_text)
	{
		Text = "";

		var stream = Game.Instance.Client.GetStream();
		NetUtil.Write(stream, new_text);
	}
}


