using core.cards;
using Godot;
using System;

public partial class CardEditWindow : Window
{
	#region Signals

	[Signal]
	public delegate void CardEditedEventHandler(string oldName, Wrapper<CardData> cardW);

	#endregion
	
	#region Nodes
	
	public Card CardNode { get; private set; }
	
	public LineEdit NameEditNode { get; private set; }
	public SpinBox CostEditNode { get; private set; }
	public LineEdit TypeEditNode { get; private set; }
	public TextEdit TextEditNode { get; private set; }
	public SpinBox PowerEditNode { get; private set; }
	public SpinBox LifeEditNode { get; private set; }
	
	#endregion
	
	private CardData? _edited;
	
	public override void _Ready()
	{
		#region Node fetching
		
		CardNode = GetNode<Card>("%Card");

		NameEditNode = GetNode<LineEdit>("%NameEdit");
		CostEditNode = GetNode<SpinBox>("%CostEdit");	
		TypeEditNode = GetNode<LineEdit>("%TypeEdit");
		TextEditNode = GetNode<TextEdit>("%TextEdit");
		PowerEditNode = GetNode<SpinBox>("%PowerEdit");
		LifeEditNode = GetNode<SpinBox>("%LifeEdit");
	
		#endregion

	}
	
	public void Edit(CardData? card=null) {
		_edited = card;
		var c = _edited ?? new();

		NameEditNode.Text = c.Name;
		CostEditNode.Value = c.Cost;
		TypeEditNode.Text = c.Type;
		TextEditNode.Text = c.Text;
		PowerEditNode.Value = c.Power;
		LifeEditNode.Value = c.Life;

		_changed();
		Show();
	}

	private void _changed() {
		CardNode.Load(Baked);
	}

	private CardData Baked {
		get {
			var c = new CardData();
			
			c.Name = NameEditNode.Text;
			c.Cost = (int)CostEditNode.Value;
			c.Type = TypeEditNode.Text;
			c.Text = TextEditNode.Text;
			c.Power = (int)PowerEditNode.Value;
			c.Life = (int)LifeEditNode.Value;
			
			c.Script = "";
			c.Expansions = new();
			
			// TODO remove
			if (_edited is not null) {
				c.Script = _edited.Script;
				c.Expansions = _edited.Expansions;
			}
			
			return c;
		}
	}

	#region Signal connections
	
	private void _on_close_requested()
	{
		Hide();
	}
	
	private void _on_name_edit_text_changed(string new_text)
	{
		_changed();
	}

	private void _on_type_edit_text_changed(string new_text)
	{
		_changed();
	}

	private void _on_text_edit_text_changed()
	{
		_changed();
	}

	private void _on_cost_edit_value_changed(double value)
	{
		_changed();
	}

	private void _on_power_edit_value_changed(double value)
	{
		_changed();
	}

	private void _on_life_edit_value_changed(double value)
	{
		_changed();
	}

	private void _on_cancel_button_pressed()
	{
		Hide();
	}

	private void _on_save_button_pressed()
	{
		var c = Baked;
		
		// TODO check correctness of the data
		var oldName = "";
		if (_edited is not null) oldName = _edited.Name;
		EmitSignal(SignalName.CardEdited, oldName, new Wrapper<CardData>(c));
	
		Hide();
	}

	#endregion
}
