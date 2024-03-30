import requests
import time
import websocket
import json
from sys import argv
# import rel

BASE_URL = 'http://localhost:5239/api/v1/'


WS_URL = 'ws://localhost:5239/api/v1/'

LOGIN_DATA = {
    'username': 'admin',
    'password': 'password',
}
PLAYER_CONFIG = {
    "botConfig": None
}
BOT_CONFIG = {
    "botConfig":{
        "strDeck":"dev::Mana Drill#3|dev::Brute#3|dev::Mage Initiate#3|dev::Warrior Initiate#3|dev::Rogue Initiate#3|dev::Urakshi Shaman#3|dev::Urakshi Raider#3|dev::Give Strength#3|dev::Blood for Knowledge#3|dev::Dragotha Mage#3|dev::Prophecy Scholar#3|dev::Trained Knight#3|dev::Cast Armor#3|dev::Druid Outcast#3|starters::Knowledge Tower#3|dev::Elven Idealist#3|dev::Elven Outcast#3|dev::Barracks#3|dev::Shieldmate#3|dev::Healer Initiate#3|dev::Archdemon Priest#3|dev::Kobold Warrior#3|dev::Kobold Mage#3|dev::Kobold Rogue#3|starters::Dragotha Student#3|starters::Tutoring Sphinx#3|starters::Dragotha Battlemage#3|starters::Inspiration#3",
        "name":"bot-player2",
        "botType":0,
        "actionDelay":0
    }
}
CREATE_DATA = {
    "mConfig": "65ffe09e593c39b101fd0b92",
    "canWatch": False,
    "p1Config": PLAYER_CONFIG,
    "p2Config": BOT_CONFIG
}

AMOUNT = 3
resp = requests.request('post', BASE_URL + 'auth/login', json=LOGIN_DATA)
token = resp.json()['token']
HEADERS = {'Authorization': f'Bearer {token}'}

def create_match(bot_count: int):
    data = {
        "mConfig": "65ffe09e593c39b101fd0b92",
        "canWatch": False,
        "p1Config": PLAYER_CONFIG,
        "p2Config": PLAYER_CONFIG
    }
    for i in range(bot_count):
        data[f'p{i+1}Config'] = BOT_CONFIG

    resp = requests.request('post', BASE_URL + 'match/create', json=data, headers=HEADERS)
    print(resp.status_code)
    print(resp.text)
    return resp.json()['id']

def config_receive_count(connections):
    result = 0
    for conn in connections:
        if conn.config_received:
            result += 1
    return result
    
def first_state_receive_count(connections):
    result = 0
    for conn in connections:
        if conn.config_received:
            result += 1
    return result


if __name__ == '__main__':
    connections = []

    match_id = create_match(int(argv[1]))
    print(match_id)

# def connect_to_match(match_id: str):
#     global connections
#     connection1 = WebSocketConnection(match_id)
#     # connection2 = WebSocketConnection(match_id)
#     connections += [connection1]
#     # connections += [connection2]

# for i in range(AMOUNT):
#     match_id = create_match(2)
#     connect_to_match(match_id)
# while True:
#     crc = config_receive_count()
#     gsrc = first_state_receive_count()
#     print(f'Config receive: {crc}\tFirst state receive: {gsrc}')
#     if crc == AMOUNT and gsrc == AMOUNT:
#         break
#     time.sleep(50)
