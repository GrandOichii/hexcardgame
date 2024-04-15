const signalr = require('@microsoft/signalr')
const reader = require('prompt-sync')()

var conn = new signalr.HubConnectionBuilder()
    .withUrl('http://localhost:5239/api/v1/test/hub')
    .build();

// conn.on('Confirm', async () => {
//     console.log('confirmed connection');
// })

// conn.on('MatchId', async () => {
//     return process.argv[2]
// })

conn.on('response', async data => {
    console.log(data);
})

conn.start()
    .then(() => {
        // conn.send('Public')
        conn.send('Private')
    })

