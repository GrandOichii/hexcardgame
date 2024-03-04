using Godot;
using System;
using System.Collections.Generic;

public partial class AuthTab : Control
{
	#region Nodes
	
	public Label JwtTokenLabelNode { get; private set; }
	public LineEdit UsernameEditNode { get; private set; }
	public LineEdit PasswordEditNode { get; private set; }
	public HttpRequest LoginRequest { get; private set; }
	public HttpRequest RegisterRequest { get; private set; }
	
	#endregion
	
	private static string ToJwtLabelText(string jwt) => string.IsNullOrEmpty(jwt) ? "Not logged in" : $"Jwt: {jwt}";
	public string Username => UsernameEditNode.Text;
	public string Password => PasswordEditNode.Text;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		JwtTokenLabelNode = GetNode<Label>("%JwtTokenLabel");
		UsernameEditNode = GetNode<LineEdit>("%UsernameEdit");


		PasswordEditNode = GetNode<LineEdit>("%PasswordEdit");
		
		#endregion
		
		JwtTokenLabelNode.Text = ToJwtLabelText("");
	}
	
	#region Signal connections

	private void OnLoginButtonPressed()
	{
		if (string.IsNullOrEmpty(Username)) {
			// TODO alert
			return;
		}
		if (string.IsNullOrEmpty(Password)) {
			// TODO alert
			return;
		}

		var data = new Dictionary<string, string>() {
			{ "Username", Username },
			{ "Password", Password },
		};

		// TODO send request
	}
	
	#endregion

}

