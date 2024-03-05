using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

public partial class AuthTab : Control
{
	#region Nodes
	
	public Label JwtTokenLabelNode { get; private set; }
	public LineEdit UsernameEditNode { get; private set; }
	public LineEdit PasswordEditNode { get; private set; }
	public HttpRequest LoginRequestNode { get; private set; }
	public HttpRequest RegisterRequestNode { get; private set; }
	
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
		
		LoginRequestNode = GetNode<HttpRequest>("%LoginRequest");
		RegisterRequestNode = GetNode<HttpRequest>("%RegisterRequest");

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
		string[] headers = new string[] { "Content-Type: application/json" };
		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		LoginRequestNode.Request(baseUrl + "auth/login", headers, HttpClient.Method.Post, JsonSerializer.Serialize(data));
	}

	private void OnLoginRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		// TODO check response code
		
		var jwt = Encoding.UTF8.GetString(body);
		GD.Print(jwt);
	}
	
	#endregion

}


