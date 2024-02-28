using Godot;
using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utility;

namespace HexClient.Match;

public partial class ConnectedMatch : Control
{
	#region Nodes
	
	public Match MatchNode { get; private set; }
	
	#endregion
	
	public IConnection Connection { get; private set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		MatchNode = GetNode<Match>("%Match");
		
		#endregion
	}

	public async Task LoadConnection(IConnection connection) {
		Connection = connection;

		// TODO load configuration
		Connection.SubscribeToUpdate(OnMatchUpdate);

		// TODO choose name
		await Connection.Write("connected-player");

		// TODO choose deck
		await Connection.Write("dev::Mana Drill#3|dev::Brute#3|dev::Mage Initiate#3|dev::Warrior Initiate#3|dev::Rogue Initiate#3|dev::Flame Eruption#3|dev::Urakshi Shaman#3|dev::Urakshi Raider#3|dev::Give Strength#3|dev::Blood for Knowledge#3|dev::Dragotha Mage#3|dev::Prophecy Scholar#3|dev::Trained Knight#3|dev::Cast Armor#3|dev::Druid Outcast#3|starters::Knowledge Tower#3|dev::Elven Idealist#3|dev::Elven Outcast#3|dev::Dub#3|dev::Barracks#3|dev::Shieldmate#3|dev::Healer Initiate#3|dev::Archdemon Priest#3|starters::Scorch the Earth#3|dev::Kobold Warrior#3|dev::Kobold Mage#3|dev::Kobold Rogue#3|starters::Dragotha Student#3|starters::Tutoring Sphinx#3|starters::Dragotha Battlemage#3|starters::Inspiration#3");
	}

	private Task OnMatchUpdate(string message) {
		if (message == "deck") return Task.CompletedTask;
		if (message == "name") return Task.CompletedTask;
		if (message == "matchstart") return Task.CompletedTask;
		
		var state = JsonSerializer.Deserialize<MatchState>(message, Common.JSON_SERIALIZATION_OPTIONS);
		GD.Print("curplayerid: " + state.CurPlayerID);

		return Task.CompletedTask;
	}
}
