using core.match.states;
using Godot;
using System;

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
	
	public void Load(MCardState card) {
		_card = card;

		var c = CardFetcher.Instance.Get(card.ID);
		var mCard = card.WithModifications(c);
		
		NameLabel.Text = mCard.Card.Name;
		if (card.MID.Length > 0)
			NameLabel.Text += " [" + card.MID + "]";
		TypeLabel.Text = mCard.Card.Type;
		TextLabel.Text = mCard.Card.Text;
		CostLabel.Text = mCard.Card.Cost.ToString();
		var powerS = "";
		if (mCard.Card.Power > 0)
			powerS = mCard.Card.Power.ToString();
		PowerLabel.Text = powerS;
		
		var lifeS = "";
		if (mCard.Card.Power > 0)
			lifeS = mCard.Card.Life .ToString();
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
}
