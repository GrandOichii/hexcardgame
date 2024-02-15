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

client.connect('http://localhost:5239/api/v1/wstest/connect');