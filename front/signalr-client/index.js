const signalr = require('@microsoft/signalr')
const reader = require('prompt-sync')()

var conn = new signalr.HubConnectionBuilder()
    .withUrl('http://localhost:5239/match-hub')
    .build();

// conn.on('GetMessage', async () => {
//     const result = reader('Enter response: ')
//     return result
// })

// conn.on('send', data => {
//     // reader
//     console.log('Received message: ' + data);
// })

conn.on('GameMethod', data => {
    console.log(data);
})

conn.on('Respond', async () => {
    const result = reader('Enter response: ');
    return result;
    // return 'pass';
})

conn.start()
    .then(() => {
        conn.send('Connect', 'mogus')
    })

