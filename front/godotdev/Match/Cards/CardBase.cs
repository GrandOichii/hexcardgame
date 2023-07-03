using core.match.states;
using Godot;
using System;

public partial class CardBase : Panel
{
	private MCardState _card;	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// OS.("Hello, world");
		
	}
	
	public void Load(MCardState card) {
		_card = card;

		var c = CardFetcher.Instance.Get(card.ID);
		GetNode<Label>("HBoxContainer/NameLabel").Text = c.Name;
		GetNode<Label>("HBoxContainer/TypeLabel").Text = c.Type;
		GetNode<Label>("HBoxContainer/TextLabel").Text = c.Text;
		GetNode<Label>("CostLabel").Text = c.Cost.ToString();
		var powerS = "";
		if (c.Power > 0)
			powerS = c.Power.ToString();
		GetNode<Label>("PowerLabel").Text = powerS;
		
		var lifeS = "";
		if (c.Power > 0)
			lifeS = c.Life .ToString();
		GetNode<Label>("LifeLabel").Text = lifeS;
		
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
//	public override void _Process(double delta)
//	{
//	}
}
