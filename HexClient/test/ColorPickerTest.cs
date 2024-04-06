using Godot;
using System;

public partial class ColorPickerTest : Control
{
	[Export]
	private PackedScene PS { get ;set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print(CustomMinimumSize);
		var child = PS.Instantiate() as Control;
		GD.Print(child.CustomMinimumSize);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnColorPickerButtonColorChanged(Color color)
	{
		GD.Print("color: " + color);
	}
}

