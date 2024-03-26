using Godot;
using HexCore.GameMatch.States;
using System;

namespace HexClient.Match;

public partial class HandCard : Control, IHandCard
{
	#region Nodes
	
	public Card CardNode { get; private set; }

	#endregion

	private MatchCardState _state;

	#nullable enable
	private CommandProcessor? _processor = null;
	#nullable disable

	private bool _mouseOver = false;

	public override void _Ready()
	{
		#region Node fetching
		
		CardNode = GetNode<Card>("%Card");
		
		#endregion
		
		CustomMinimumSize = CardNode.Size * CardNode.Scale;
	}

	public void SetCommandProcessor(CommandProcessor processor) {
		_processor = processor;
	}

	public void Load(MatchCardState state)
	{
		_state = state;
		CardNode.Load(state);
	}

	public void SetShowCardIds(bool v)
	{
		CardNode.SetShowMID(v);
	}

	public MatchCardState GetState()
	{
		return _state;
	}

	private void Unfocus() {
		CardNode.Unfocus();
	}

	private void Check() {
		if (_processor is null) {
			return;
		}

		if (!_mouseOver) {
			Unfocus();
			return;
		}

		if (!_processor.Accepts(this)) {
			Unfocus();
			return;
		}

		CardNode.Focus();
	}

	private void TryAddToAction() {
		if (!_processor.Accepts(this)) return;

		_processor.Process(this);

		Check();
	}

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed("cancel-command"))
			Check();
	}
	
	#region Signal connections
	
	private void OnCardMouseEntered()
	{
		_mouseOver = true;
		Check();
	}

	private void OnCardMouseExited()
	{
		_mouseOver = false;
		Unfocus();		
	}

	private void OnCardGuiInput(InputEvent e)
	{
		if (e.IsActionPressed("add-to-action")) {
			TryAddToAction();
		}

	}
	
	#endregion
}

