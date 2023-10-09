using core.match.states;
using Godot;
using System;

public partial class PlayerInfo : Control
{
	private static Random _rnd = new();
	
	#region Packed scenes
	
	private readonly static PackedScene DiscardedCardPS = ResourceLoader.Load<PackedScene>("res://Match/Cards/DiscardedCard.tscn");
	
	#endregion

	#region Exports
	
	[Export]
	public Color CurrentPlayerColor { get; set; }
	
	#endregion

	#region Nodes

	public VBoxContainer DiscardContainerNode { get; private set; }
	public PanelContainer BgNode { get; private set; }
	public Label NameLabelNode { get; private set; }
	public Label EnergyLabelNode { get; private set; }
	public Control DiscardScrollNode { get; private set; }

	#endregion
	
	private Color _defaultBgColor;

	private MatchState _state;
	public int PlayerI { get; set; }

	public override void _Ready()
	{
		#region Node fetching

		DiscardContainerNode = GetNode<VBoxContainer>("%DiscardContainer");
		BgNode = GetNode<PanelContainer>("%Bg");
		NameLabelNode = GetNode<Label>("%NameLabel");
		EnergyLabelNode = GetNode<Label>("%EnergyLabel");
		DiscardScrollNode = GetNode<Control>("%DiscardScroll");

		#endregion
		
		BgNode.Set("theme_override_styles/panel", _bgStyle.Duplicate());

		_defaultBgColor = BgColor;

		// populate discard
//		var count = _rnd.Next(10);
//		for (int i = 0; i < 10; i++) {
//			var card = DiscardedCardPS.Instantiate() as DiscardedCard;
//			DiscardContainerNode.AddChild(card);
//		}
	}
	
	public Color BgColor {
		get => _bgStyle.BgColor;
		set {
			_bgStyle.BgColor = value;
		}
	}

	private StyleBoxFlat _bgStyle => BgNode.Get("theme_override_styles/panel").As<StyleBoxFlat>();
	
	public void UpdateState(MatchState state) {
		_state = state;

		var pState = state.Players[PlayerI];
		NameLabelNode.Text = pState.Name;
		EnergyLabelNode.Text = pState.Energy.ToString();
		BgColor = state.CurPlayerID == pState.ID ? CurrentPlayerColor : _defaultBgColor;

		// TODO discard
		var cCount = DiscardContainerNode.GetChildCount();
		var nCount = _state.Players[PlayerI].Discard.Count;

		if (nCount > cCount) {
			// fill discard up to new count
			for (int i = 0; i < nCount - cCount; i++) {
				var child = DiscardedCardPS.Instantiate() as DiscardedCard;
				DiscardContainerNode.AddChild(child);
			}
		}
		if (nCount < cCount) {
			// trim child count
			for (int i = nCount + 1; i < cCount; i++) {
				var child = DiscardContainerNode.GetChild(i);
				child.QueueFree();
			}
		}

		// load card data
		for (int i = nCount - 1; i >= 0; --i) {
			(DiscardContainerNode.GetChild(i) as DiscardedCard).Load(_state.Players[PlayerI].Discard[i]);
		}
	}
	
	#region Signal connections

	private void _on_toggle_discard_button_pressed()
	{
		DiscardScrollNode.Visible = !DiscardScrollNode.Visible;
	}
	
	#endregion
}

