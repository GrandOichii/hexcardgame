using Godot;
using System;

namespace HexClient;

public partial class GlobalSettings : Node
{
	// TODO allow to set
	public string BaseUrl { get; set; } = "localhost:5239";
	public string ApiUrl { get; set; } = "http://localhost:5239/api/v1/";
	public string JwtToken { get; set; }
	
	private bool _isAdmin;
	public bool IsAdmin {
		get => _isAdmin;
		set {
			_isAdmin = value;
			foreach (var node in GetTree().GetNodesInGroup("admin_ui"))
				(node as Control).Visible = value;
		}
	} 
	
	public override void _Ready()
	{
		IsAdmin = false;
	}
}
