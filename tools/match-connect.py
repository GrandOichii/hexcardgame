import requests
import websocket
import json
# import rel

BASE_URL = 'http://localhost:5239/api/v1/'
WS_URL = 'ws://localhost:5239/api/v1/'
LOGIN_DATA = {
    'username': 'admin',
    'password': 'password',
}
CREATE_DATA = json.loads('{"mConfig":"65ffe09e593c39b101fd0b92","canWatch":false,"p1Config":{"botConfig":null},"p2Config":{"botConfig":{"strDeck":"dev::Mana Drill#3|dev::Brute#3|dev::Mage Initiate#3|dev::Warrior Initiate#3|dev::Rogue Initiate#3|dev::Urakshi Shaman#3|dev::Urakshi Raider#3|dev::Give Strength#3|dev::Blood for Knowledge#3|dev::Dragotha Mage#3|dev::Prophecy Scholar#3|dev::Trained Knight#3|dev::Cast Armor#3|dev::Druid Outcast#3|starters::Knowledge Tower#3|dev::Elven Idealist#3|dev::Elven Outcast#3|dev::Barracks#3|dev::Shieldmate#3|dev::Healer Initiate#3|dev::Archdemon Priest#3|dev::Kobold Warrior#3|dev::Kobold Mage#3|dev::Kobold Rogue#3|starters::Dragotha Student#3|starters::Tutoring Sphinx#3|starters::Dragotha Battlemage#3|starters::Inspiration#3","name":"bot-player2","botType":0,"actionDelay":0}}}')

resp = requests.request('post', BASE_URL + 'auth/login', json=LOGIN_DATA)
token = resp.json()['token']

headers = {'Authorization': f'Bearer {token}'}
resp = requests.request('post', BASE_URL + 'match/create', json=CREATE_DATA, headers=headers)
match_id = resp.json()['id']

def on_open(socket: websocket.WebSocket):
    name = input('Enter name: ')
    deck = 'dev::Mana Drill#3|dev::Brute#3|dev::Mage Initiate#3|dev::Warrior Initiate#3|dev::Rogue Initiate#3|dev::Urakshi Shaman#3|dev::Urakshi Raider#3|dev::Give Strength#3|dev::Blood for Knowledge#3|dev::Dragotha Mage#3|dev::Prophecy Scholar#3|dev::Trained Knight#3|dev::Cast Armor#3|dev::Druid Outcast#3|starters::Knowledge Tower#3|dev::Elven Idealist#3|dev::Elven Outcast#3|dev::Barracks#3|dev::Shieldmate#3|dev::Healer Initiate#3|dev::Archdemon Priest#3|dev::Kobold Warrior#3|dev::Kobold Mage#3|dev::Kobold Rogue#3|starters::Dragotha Student#3|starters::Tutoring Sphinx#3|starters::Dragotha Battlemage#3|starters::Inspiration#3'
    socket.send(
        json.dumps({
            'name': name,
            'deck': deck,
        })
    )
    print('sent data')
    # socket.close()
    # exit(0)

def on_message(socket: websocket.WebSocket, message: str):
    if message == 'ping':
        print('received check, responding...')
        socket.send('pong')
        return
    print(message)
    if message.startswith('config-'):
        return
    resp = input('Enter response: ')
    socket.send(resp)

def on_close(socket: websocket.WebSocket):
    print("Connection closed")

socket = websocket.WebSocketApp(WS_URL + 'match/connect/' + match_id,
                            on_open=on_open,
                            on_message=on_message,
                            on_close=on_close)

socket.run_forever()

# async def connect_to_server():
#     async with websockets.connect(WS_URL + 'match/connect/' + match_id) as websocket:
#         response = await websocket.recv()
#         print(f"Received: {response}")
#         name = input('Enter name: ')
#         deck = '"dev::Mana Drill#3|dev::Brute#3|dev::Mage Initiate#3|dev::Warrior Initiate#3|dev::Rogue Initiate#3|dev::Urakshi Shaman#3|dev::Urakshi Raider#3|dev::Give Strength#3|dev::Blood for Knowledge#3|dev::Dragotha Mage#3|dev::Prophecy Scholar#3|dev::Trained Knight#3|dev::Cast Armor#3|dev::Druid Outcast#3|starters::Knowledge Tower#3|dev::Elven Idealist#3|dev::Elven Outcast#3|dev::Barracks#3|dev::Shieldmate#3|dev::Healer Initiate#3|dev::Archdemon Priest#3|dev::Kobold Warrior#3|dev::Kobold Mage#3|dev::Kobold Rogue#3|starters::Dragotha Student#3|starters::Tutoring Sphinx#3|starters::Dragotha Battlemage#3|starters::Inspiration#3'
#         await websocket.send(
#             json.dumps({
#                 'name': name,
#                 'deck': deck,
#             })
#         )
#         print('sent data')
# asyncio.get_event_loop().run_until_complete(connect_to_server())