using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Utility;

namespace HexClient.Manager.Tabs;

public class LoginResult {
	public required string Token { get; set; }
	public required bool IsAdmin { get; set; }
}

public partial class AuthTab : Control
{
	#region Nodes
	
	public Label JwtTokenLabelNode { get; private set; }
	public LineEdit UsernameEditNode { get; private set; }
	public LineEdit PasswordEditNode { get; private set; }
	public HttpRequest LoginRequestNode { get; private set; }
	public HttpRequest RegisterRequestNode { get; private set; }

	public AcceptDialog ErrorPopupNode { get; private set; }
	public ConfirmationDialog LogoutConfirmationPopupNode { get; private set; }
	
	#endregion
	
	private static string ToJwtLabelText(string jwt) => string.IsNullOrEmpty(jwt) ? "Not logged in" : "Jwt: " + Regex.Replace(jwt, "(?<=^.{20}).*", "...");

	public string Username => UsernameEditNode.Text;
	public string Password => PasswordEditNode.Text;

	private string JwtToken {
		get => GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
		set {
			GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken = value;
			JwtTokenLabelNode.Text = ToJwtLabelText(value);
		}
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		JwtTokenLabelNode = GetNode<Label>("%JwtTokenLabel");
		UsernameEditNode = GetNode<LineEdit>("%UsernameEdit");
		PasswordEditNode = GetNode<LineEdit>("%PasswordEdit");
		
		LoginRequestNode = GetNode<HttpRequest>("%LoginRequest");
		RegisterRequestNode = GetNode<HttpRequest>("%RegisterRequest");

		ErrorPopupNode = GetNode<AcceptDialog>("%ErrorPopup");
		LogoutConfirmationPopupNode = GetNode<ConfirmationDialog>("%LogoutConfirmationPopup");

		#endregion
		
		JwtTokenLabelNode.Text = ToJwtLabelText("");
		if (!string.IsNullOrEmpty(UsernameEditNode.Text)) {
			OnLoginButtonPressed();
		}
	}
	
	#region Signal connections

	private void OnLoginButtonPressed()
	{
		if (string.IsNullOrEmpty(Username)) {
			ErrorPopupNode.DialogText = "Enter username";
			ErrorPopupNode.Show();
			return;
		}
		if (string.IsNullOrEmpty(Password)) {
			ErrorPopupNode.DialogText = "Enter password";
			ErrorPopupNode.Show();
			return;
		}

		var data = new Dictionary<string, string>() {
			{ "Username", Username },
			{ "Password", Password },
		};

		string[] headers = new string[] { "Content-Type: application/json" };
		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl;
		LoginRequestNode.Request(baseUrl + "auth/login", headers, HttpClient.Method.Post, JsonSerializer.Serialize(data));
	}

	private void OnLoginRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			ErrorPopupNode.DialogText = $"Failed to login! (Response code: {response_code})\nResponse body:\n{resp}";
			ErrorPopupNode.Show();
			return;
		}
		
		var login = JsonSerializer.Deserialize<LoginResult>(body, Common.JSON_SERIALIZATION_OPTIONS);

		JwtToken = login.Token;
		JwtTokenLabelNode.Text = ToJwtLabelText(login.Token);

		GetNode<GlobalSettings>("/root/GlobalSettings").IsAdmin = login.IsAdmin;
	}

	private void OnLogoutButtonPressed()
	{
		if (string.IsNullOrEmpty(GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken))
			return;
		LogoutConfirmationPopupNode.Show();		
	}

	private void OnLogoutConfirmationPopupConfirmed()
	{
		JwtToken = "";
		JwtTokenLabelNode.Text = ToJwtLabelText(JwtToken);

		GetNode<GlobalSettings>("/root/GlobalSettings").IsAdmin = false;
	}
	
	#endregion
}
