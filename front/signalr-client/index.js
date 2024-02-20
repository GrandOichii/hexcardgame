const signalr = require('@microsoft/signalr')
const reader = require('prompt-sync')()

var conn = new signalr.HubConnectionBuilder()
    .withUrl('http://localhost:5239/api/v1/match/watch')
    .build();

// conn.on('Confirm', async () => {
//     console.log('confirmed connection');
// })

// conn.on('MatchId', async () => {
//     return process.argv[2]
// })

conn.on('Update', async data => {
    console.log('Match update:');
    console.log(data);
    console.log();
})

conn.start()
    .then(() => {
        conn.send('Connect', process.argv[2])
        console.log('connected');
    })

