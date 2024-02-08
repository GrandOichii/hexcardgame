const signalr = require('@microsoft/signalr')

var conn = new signalr.HubConnectionBuilder()
    .withUrl('http://localhost:5121/chat-hub')
    .build();

conn.on('send', data => {
    console.log(data);
})

conn.start()
    .then(() => conn.send('Hello there'))