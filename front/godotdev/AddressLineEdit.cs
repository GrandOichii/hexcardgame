using Godot;
using Shared;
using System;

using System.Net.Sockets;

public partial class AddressLineEdit : LineEdit
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private async void _on_connect_button_pressed()
	{
		try {
			var text = Text;
			var split = text.Split(":");
			if (split.Length != 2) {
				throw new Exception("");
			}
			var address = split[0];
			var port = int.Parse(split[1]);

			var client = Game.Instance.Client;
			client.Connect(address, port);

//			OS.Alert("Conected, message: " + message, "Connected");
			
			GetTree().ChangeSceneToFile("res://match.tscn");

		} catch (Exception e) {
			OS.Alert("Failed to connect: " + e);
		}
		// Replace with function body.
	}
}


