using Godot;
using System;

namespace HexClient.Utility;

public partial class Wrapper<T> : Node {
	public T Value { get; set; }
	public Wrapper(T v) { Value = v; }
}
