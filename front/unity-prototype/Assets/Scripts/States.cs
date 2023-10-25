using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PlayerState {
    public int deckCount;
    public int energy;
    public string id;
    public string name;
}


[Serializable]
public struct MatchState {
    public string curPlayerID;
    public List<string> args;
    public string request;
    public List<PlayerState> players;


    public static MatchState FromJson(string message) {
        return JsonUtility.FromJson<MatchState>(message);
    }
}


//     "map": {
//         "tiles": [
//             [
//                 null,
//                 {
//                     "entity": {
//                         "can": [],
//                         "cost": 10,
//                         "defence": 0,
//                         "hasDefence": false,
//                         "id": "dev::Castle",
//                         "life": 10,
//                         "mid": "61",
//                         "movement": -1,
//                         "name": "Castle",
//                         "ownerID": "1",
//                         "power": -1,
//                         "text": "When [CARDNAME] is destroyed, you lose the match.",
//                         "type": "Structure"
//                     },
//                     "hasGrave": false,
//                     "ownerID": "1"
//                 },
//                 null,
//                 null
//             ],
//             [
//                 null,
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": "1"
//                 },
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": "1"
//                 },
//                 null
//             ],
//             [
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": ""
//                 },
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": "1"
//                 },
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": ""
//                 },
//                 null
//             ],
//             [
//                 null,
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": ""
//                 },
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": ""
//                 },
//                 null
//             ],
//             [
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": ""
//                 },
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": ""
//                 },
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": ""
//                 },
//                 null
//             ],
//             [
//                 null,
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": ""
//                 },
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": ""
//                 },
//                 null
//             ],
//             [
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": ""
//                 },
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": "2"
//                 },
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": ""
//                 },
//                 null
//             ],
//             [
//                 null,
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": "2"
//                 },
//                 {
//                     "entity": null,
//                     "hasGrave": false,
//                     "ownerID": "2"
//                 },
//                 null
//             ],
//             [
//                 null,
//                 {
//                     "entity": {
//                         "can": [],
//                         "cost": 10,
//                         "defence": 0,
//                         "hasDefence": false,
//                         "id": "dev::Castle",
//                         "life": 10,
//                         "mid": "62",
//                         "movement": -1,
//                         "name": "Castle",
//                         "ownerID": "2",
//                         "power": -1,
//                         "text": "When [CARDNAME] is destroyed, you lose the match.",
//                         "type": "Structure"
//                     },
//                     "hasGrave": false,
//                     "ownerID": "2"
//                 },
//                 null,
//                 null
//             ]
//         ]
//     },
//     "myData": {
//         "hand": [
//             {
//                 "can": [],
//                 "cost": 3,
//                 "defence": 0,
//                 "hasDefence": false,
//                 "id": "dev::Healer Initiate",
//                 "life": 3,
//                 "mid": "30",
//                 "movement": 0,
//                 "name": "Healer Initiate",
//                 "ownerID": "1",
//                 "power": 2,
//                 "text": "{Virtuous}",
//                 "type": "Unit"
//             },
//             {
//                 "can": [],
//                 "cost": 2,
//                 "defence": 0,
//                 "hasDefence": false,
//                 "id": "dev::Archdemon Priest",
//                 "life": 2,
//                 "mid": "6",
//                 "movement": 0,
//                 "name": "Archdemon Priest",
//                 "ownerID": "1",
//                 "power": 2,
//                 "text": "{Vile}",
//                 "type": "Unit"
//             },
//             {
//                 "can": [],
//                 "cost": 3,
//                 "defence": 0,
//                 "hasDefence": false,
//                 "id": "dev::Healer Initiate",
//                 "life": 3,
//                 "mid": "29",
//                 "movement": 0,
//                 "name": "Healer Initiate",
//                 "ownerID": "1",
//                 "power": 2,
//                 "text": "{Virtuous}",
//                 "type": "Unit"
//             },
//             {
//                 "can": [],
//                 "cost": 2,
//                 "defence": 0,
//                 "hasDefence": false,
//                 "id": "dev::Archdemon Priest",
//                 "life": 2,
//                 "mid": "9",
//                 "movement": 0,
//                 "name": "Archdemon Priest",
//                 "ownerID": "1",
//                 "power": 2,
//                 "text": "{Vile}",
//                 "type": "Unit"
//             }
//         ],
//         "id": "1"
//     },
//     "newLogs": [
//         [
//             {
//                 "cardRef": "",
//                 "text": "P1 drew 3 cards."
//             }
//         ],
//         [
//             {
//                 "cardRef": "",
//                 "text": "P2 drew 3 cards."
//             }
//         ],
//         [
//             {
//                 "cardRef": "",
//                 "text": "Match started"
//             }
//         ],
//         [
//             {
//                 "cardRef": "",
//                 "text": "P1 started their turn."
//             }
//         ],
//         [
//             {
//                 "cardRef": "",
//                 "text": "P1 drew 1 card."
//             }
//         ]
//     ],
//     "players": [
//         {
//             "deckCount": 26,
//             "discard": [],
//             "energy": 1,
//             "handCount": 4,
//             "id": "1",
//             "name": "P1"
//         },
//         {
//             "deckCount": 27,
//             "discard": [],
//             "energy": 0,
//             "handCount": 3,
//             "id": "2",
//             "name": "P2"
//         }
//     ],
// }
