const { connect } = require('http2');

var WebSocketClient = require('websocket').client;

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
            console.log("Received: '" + message.utf8Data + "'");
            if (data == 'handshake') {
                connection.send('handshake')
                return
            }
            if (data == 'ping') {
                connection.send('pong')
                return
            }
            connection.send('some data')
        }
    });
});

client.connect('http://localhost:5239/api/v1/wstest/connect');