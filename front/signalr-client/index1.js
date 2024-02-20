const signalr = require('@microsoft/signalr')
const reader = require('prompt-sync')()

var conn = new signalr.HubConnectionBuilder()
    .withUrl('http://localhost:5239/api/v1/match/live')
    .build();

conn.on('Initial', async data => {
    console.log('Received: ' + data);
})

conn.on('Confirm', async () => {
    console.log('confirmed connection');
})

conn.on('Update', async match => {
    console.log('Match update:');
    console.log(match);
    console.log();
})

conn.start()
    .then(() => {
        // conn.send('Connect', 'mogus')
    })

