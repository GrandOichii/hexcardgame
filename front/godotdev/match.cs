using Godot;
using System;
using Shared;

public partial class match : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var message = NetUtil.Read(Game.Instance.Client.GetStream());
		OS.Alert(message);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
