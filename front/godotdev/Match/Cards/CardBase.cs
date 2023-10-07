using Godot;
using System;
using System.Text.RegularExpressions;


using core.cards;
using core.match.states;
using System.Collections.Generic;
using System.Text;

public partial class CardBase : Node2D
{
	static private readonly Dictionary<Regex, Func<string, string, string>> REGEX_MAP = new() {
		{new Regex("\\{([^\\{]+)\\}"), (cardName, text) => {
			return string.Concat("[color=red]", text.AsSpan(1, text.Length-2), "[/color]");
		}},
		{new Regex("\\[([^\\[]+)\\]"), (cardName, text) => {
			var result = "[color=orange]{0}[/color]";
			string replacement = cardName;
			if (text != "[CARDNAME]")
				replacement = text[1..^1];
			return string.Format(result, replacement);
		}},
	};

	// nodes
	public ColorRect Bg { get; private set; }
	public ColorRect Fg { get; private set; }
	public Control MainCardNode { get; private set; }
	private Label NameLabel;
	private Label TypeLabel;
	private Label CostLabel;
	private Label PowerLabel;
	private Label LifeLabel;
	private TextureRect ImageNode;
	private RichTextLabel TextNode;
	
	public MCardState LastState { get; private set; }
	
	public override void _Ready()
	{
		// node fetching
		Bg = GetNode<ColorRect>("%Bg");
		Fg = GetNode<ColorRect>("%Fg");
		NameLabel = GetNode<Label>("%Name");
		TypeLabel = GetNode<Label>("%Type");
		CostLabel = GetNode<Label>("%Cost");
		PowerLabel = GetNode<Label>("%Power");
		LifeLabel = GetNode<Label>("%Life");
		ImageNode = GetNode<TextureRect>("%Image");
		TextNode = GetNode<RichTextLabel>("%Text");
		MainCardNode = GetNode<Control>("%MainCard");
		
	}
	
	public void Load(MCardState card) {
		// _card = card;
		LastState = card;
		
		NameLabel.Text = card.Name;
		if (card.MID.Length > 0)
			NameLabel.Text += " [" + card.MID + "]";
		TypeLabel.Text = card.Type;
		CostLabel.Text = "(" + card.Cost.ToString() + ")";
		var powerS = "";
		if (card.Power > 0)
			powerS = card.Power.ToString();
		PowerLabel.Text = powerS;
		
		var lifeS = "";
		if (card.Power > 0)
			lifeS = card.Life .ToString();
		LifeLabel.Text = lifeS;

		// text loading
		List<TextReplacer> replacers = new();
		foreach (var pair in REGEX_MAP ) {
			var re = pair.Key;
			var operand = pair.Value;
			var match = re.Matches(card.Text);
			for (int i = match.Count - 1; i >= 0; i--) {
				System.Text.RegularExpressions.Match m = match[i];
				TextReplacer replacer;
				replacer.StartIndex = m.Index;
				replacer.Original = m.Value;
				replacer.New = operand(card.Name, m.Value);
				replacers.Add(replacer);

			}
		}
		replacers.Sort((TextReplacer r1, TextReplacer r2) => {
			return r2.StartIndex.CompareTo(r1.StartIndex);
		});
		StringBuilder sb = new(card.Text);
		foreach (var replacer in replacers)
			replacer.Modifiy(ref sb);
		TextNode.Text = sb.ToString();
	}
	
	public void Load(Card card) {
		Load(new MCardState(card));
	}
}


struct TextReplacer {
	public int StartIndex;
	public string Original;
	public string New;

	public void Modifiy(ref StringBuilder s) {
		// s = s.Replace(Original, New, StartIndex, 1);
		s = s.Replace(Original, New, StartIndex, Original.Length);
	}
}
