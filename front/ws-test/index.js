const reader = require('prompt-sync')();
const WebSocketClient = require('websocket').client;

var client = new WebSocketClient();

client.on('connectFailed', function(error) {
    console.log('Connect Error: ' + error.toString());
});

client.on('connect', connection => {
    console.log('WebSocket Client Connected');
    connection.on('error', error => {
        console.log("Connection Error: " + error.toString());
    });
    connection.on('close', () => {
        console.log('Connection Closed');
    });
    connection.on('message', (message) => {
        if (message.type === 'utf8') {
            const data = message.utf8Data
            if (data == 'playerwaiting') {
                connection.send('accept')
                return
            }
            if (data == 'matchstart') {
                connection.send('accept');
                return
            }
            if (data == 'deck') {
                connection.send('dev::Mana Drill#3|dev::Brute#3|dev::Mage Initiate#3|dev::Warrior Initiate#3|dev::Rogue Initiate#3|dev::Flame Eruption#3|dev::Urakshi Shaman#3|dev::Urakshi Raider#3|dev::Give Strength#3|dev::Blood for Knowledge#3|dev::Dragotha Mage#3|dev::Prophecy Scholar#3|dev::Trained Knight#3|dev::Cast Armor#3|dev::Druid Outcast#3|starters::Knowledge Tower#3|dev::Elven Idealist#3|dev::Elven Outcast#3|dev::Dub#3|dev::Barracks#3|dev::Shieldmate#3|dev::Healer Initiate#3|dev::Archdemon Priest#3|starters::Scorch the Earth#3|dev::Kobold Warrior#3|dev::Kobold Mage#3|dev::Kobold Rogue#3|starters::Dragotha Student#3|starters::Tutoring Sphinx#3|starters::Dragotha Battlemage#3|starters::Inspiration#3')
                return
            }
            if (data == 'name') {
                connection.send('realplayer')
                return
            }

            console.log("Received: '" + message.utf8Data + "'");
            const state = JSON.parse(message.utf8Data)
            console.log(state.request);
            if (!state.request) return
            if (state.request == 'update') return
            // connection.send('amopgus')

            connection.send(reader('Enter server response: '))
        }
    });

    // connection.send('connect')
});

client.connect(`http://localhost:5239/api/v1/match/connect/${process.argv[2]}`);
// client.connect(`http://localhost:5239/api/v1/wstest/connect/e8b0c6d3-ae5a-46b2-ab25-be37d2342579`);
