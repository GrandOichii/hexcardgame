import requests
import time
import websocket
import json
from sys import argv
import socket
# import rel

ADDRESS = '127.0.0.1'
API_URL = 'http://' + ADDRESS + ':5239/api/v1/'

class TcpConnection:
    def __init__(self, match_id: str):
        self.config_received = False
        self.first_state_received = False
        self.open = False

        resp = requests.get(API_URL + 'match/' + match_id)
        print(resp.text)
        port = resp.json()['tcpPort']
        print(ADDRESS, port)
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.socket.connect((ADDRESS, port))
        self.socket.settimeout(.2)
        self.on_connect()
        self.run_loop()

    def on_connect(self):
        self.open = True
        name = input('Enter name: ')
        # name = 'player1'
        deck = 'dev::Mana Drill#3|dev::Brute#3|dev::Mage Initiate#3|dev::Warrior Initiate#3|dev::Rogue Initiate#3|dev::Urakshi Shaman#3|dev::Urakshi Raider#3|dev::Give Strength#3|dev::Blood for Knowledge#3|dev::Dragotha Mage#3|dev::Prophecy Scholar#3|dev::Trained Knight#3|dev::Cast Armor#3|dev::Druid Outcast#3|starters::Knowledge Tower#3|dev::Elven Idealist#3|dev::Elven Outcast#3|dev::Barracks#3|dev::Shieldmate#3|dev::Healer Initiate#3|dev::Archdemon Priest#3|dev::Kobold Warrior#3|dev::Kobold Mage#3|dev::Kobold Rogue#3|starters::Dragotha Student#3|starters::Tutoring Sphinx#3|starters::Dragotha Battlemage#3|starters::Inspiration#3'
        self.write(
            json.dumps({
                'name': name,
                'deck': deck,
            })
        )

    def write(self, msg: str):
        message_length = len(msg)
        message_length_bytes = message_length.to_bytes(4, byteorder='little')
        message_bytes = msg.encode('utf-8')
        message_with_length = message_length_bytes + message_bytes
        self.socket.sendall(message_with_length)

    def read(self):
        message = ''
        try :
            message_length_bytes = self.socket.recv(4)
            message_length = int.from_bytes(message_length_bytes, byteorder='little')

            # Receive the message itself
            while len(message) < message_length:
                message_bytes = self.socket.recv(message_length)
                message += message_bytes.decode('utf-8')

        except socket.timeout:
            message = ''

        return message
    
    def run_loop(self):
        while True:
            msg = self.read()
            if msg == '': continue
            self.respond(msg)

    def respond(self, msg: str):
        if msg == 'ping':
            self.write('pong')
            return
        if msg.startswith('config-'):
            self.config_received = True
            return
        self.first_state_received = True
        print(msg)
        data = json.loads(msg)
        if data['request'] == 'update':
            return
        resp = input('Enter response: ')
        self.write(resp)
        # self.force_close()

class WebSocketConnection:
    def force_close(self):
        self.socket.close()

    def on_open(self, socket: websocket.WebSocket):
        self.open = True
        name = input('Enter name: ')
        # name = 'player1'
        deck = 'dev::Mana Drill#3|dev::Brute#3|dev::Mage Initiate#3|dev::Warrior Initiate#3|dev::Rogue Initiate#3|dev::Urakshi Shaman#3|dev::Urakshi Raider#3|dev::Give Strength#3|dev::Blood for Knowledge#3|dev::Dragotha Mage#3|dev::Prophecy Scholar#3|dev::Trained Knight#3|dev::Cast Armor#3|dev::Druid Outcast#3|starters::Knowledge Tower#3|dev::Elven Idealist#3|dev::Elven Outcast#3|dev::Barracks#3|dev::Shieldmate#3|dev::Healer Initiate#3|dev::Archdemon Priest#3|dev::Kobold Warrior#3|dev::Kobold Mage#3|dev::Kobold Rogue#3|starters::Dragotha Student#3|starters::Tutoring Sphinx#3|starters::Dragotha Battlemage#3|starters::Inspiration#3'
        socket.send(
            json.dumps({
                'name': name,
                'deck': deck,
            })
        )

    def on_message(self, socket: websocket.WebSocket, message: str):
        if message == 'ping':
            socket.send('pong')
            return
        if message.startswith('config-'):
            self.config_received = True
            return
        self.first_state_received = True
        print(message)
        data = json.loads(message)
        if data['request'] == 'update':
            return
        resp = input('Enter response: ')
        socket.send(resp)
        # self.force_close()

    def on_close(self, socket: websocket.WebSocket):
        self.open = False

    def __init__(self, match_id: str):
        self.config_received = False
        self.first_state_received = False
        self.open = False

        self.socket = websocket.WebSocketApp(WS_URL + 'match/connect/' + match_id,
                            on_open=self.on_open,
                            on_message=self.on_message,
                            on_close=self.on_close)
        self.socket.run_forever()

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
        "actionDelay":0}
    }
CREATE_DATA = {
    "mConfig": "65ffe09e593c39b101fd0b92",
    "canWatch": False,
    "p1Config": PLAYER_CONFIG,
    "p2Config": BOT_CONFIG
}

MATCH_ID = argv[1]

conn = TcpConnection(MATCH_ID)