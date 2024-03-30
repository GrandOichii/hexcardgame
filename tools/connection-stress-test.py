from connect import WebSocketConnection, TcpConnection, API_URL
from create import create_match

AMOUNT = 20

CONNECTION_MAP ={
    'WebSocket': WebSocketConnection,
    'Tcp': TcpConnection,
}

for key, value in CONNECTION_MAP.items():
    print(f'Testing {key}')
    for i in range(AMOUNT):
        match_id = create_match(1)
        conn = value(match_id)
        print(f'\tPassed {i+1}')
