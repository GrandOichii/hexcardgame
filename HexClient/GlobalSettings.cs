using Godot;
using System;

namespace HexClient;

public partial class GlobalSettings : Node
{
	public string BaseUrl { get; set; }
	public string JwtToken { get; set; }
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
