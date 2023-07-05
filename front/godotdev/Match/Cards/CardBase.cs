using Godot;
using System;

using core.match.states;
using core.cards;

public partial class CardBase : Panel
{
	private MCardState _card;
	private Label NameLabel;
	private Label TypeLabel;
	private Label TextLabel;
	private Label CostLabel;
	private Label PowerLabel;
	private Label LifeLabel;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// OS.("Hello, world");
		NameLabel = GetNode<Label>("%NameLabel");

		TypeLabel = GetNode<Label>("%TypeLabel");
		TextLabel = GetNode<Label>("%TextLabel");
		CostLabel = GetNode<Label>("%CostLabel");
		PowerLabel = GetNode<Label>("%PowerLabel");
		LifeLabel = GetNode<Label>("%LifeLabel");
	}

	public void Load(Card card) {
		NameLabel.Text = card.Name;
		TypeLabel.Text = card.Type;
		TextLabel.Text = card.Text;
		CostLabel.Text = card.Cost.ToString();
		var powerS = "";
		if (card.Power > 0)
			powerS = card.Power.ToString();
		PowerLabel.Text = powerS;
		
		var lifeS = "";
		if (card.Power > 0)
			lifeS = card.Life .ToString();
		LifeLabel.Text = lifeS;
	}
	
	public void Load(MCardState card) {
		_card = card;
		
		NameLabel.Text = card.Name;
		if (card.MID.Length > 0)
			NameLabel.Text += " [" + card.MID + "]";
		TypeLabel.Text = card.Type;
		TextLabel.Text = card.Text;
		CostLabel.Text = card.Cost.ToString();
		var powerS = "";
		if (card.Power > 0)
			powerS = card.Power.ToString();
		PowerLabel.Text = powerS;
		
		var lifeS = "";
		if (card.Power > 0)
			lifeS = card.Life .ToString();
		LifeLabel.Text = lifeS;
		
//		TODO doesn't work for some ungodly reason
//		if (card.AvaliableActions.Count > 0)  {
//			 var style = new StyleBoxFlat();
//			// style.BorderColor = new Color(255, 0, 0);
//			// style.SetBorderWidthAll(20);
////			var style = GetThemeStylebox("normal").Duplicate() as StyleBoxFlat;
////			GD.Print(style.BorderWidthBottom.ToString());
//
//			style.BorderColor = new Color(1, 0, 0);
////			AddThemeStyleboxOverride("normal", style);
//			Set("custom_styles/panel", style);
//
////			GD.Print((GetThemeStylebox("normal").Duplicate() as StyleBoxFlat).BorderColor.ToString());
//		}
	}

//	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void OnGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton) {
			var e = @event as InputEventMouseButton;
			if (e.IsPressed() && e.ButtonIndex == MouseButton.Left) {
				var game = Game.Instance;
				var action = game.Action;
				if (action.Count == 0)  {
					game.AddToAction("play");
					game.AddToAction(_card.MID);
					return;
				}
			}
		}
	}
}




