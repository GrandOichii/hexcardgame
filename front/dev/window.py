import socket
import json
from types import SimpleNamespace

from front.dev.frame import *
from front.dev.widgets import *


def parse_state(textj):
    return json.loads(textj, object_hook=lambda d: SimpleNamespace(**d))


class ClientWindow(Window):
    def __init__(self, host, port):
        super().__init__()

        self.host = host
        self.port = port
        self.player_count = 2

        self.init_ui()
        self.set_title('dev client')


        # connection

        self.last_state = None
        # self.cursor_card_widget = CardWidget(self, [])
        self.sock = None

    def on_close(self):
        super().on_close()
        if self.sock:
            self.sock.close()

    def config_connection(self):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.connect((self.host, self.port))
        # self.sconfig = parse_state(self.read_msg())
        # self.top_player.lanes_c.set_up_lanes(self.sconfig.lane_count)
        # self.bottom_player.lanes_c.set_up_lanes(self.sconfig.lane_count)
        self.sconfig = parse_state(self.read_msg())

        self.player_count = self.sconfig.playerCount
        self.sock.settimeout(.01)

    def init_ui(self):
        # TODO remove, for easier debugging
        self.font = None
        try:
            ContentPool.Instance.load_font('basic', 'fonts/Montserrat-Thin.ttf')
            self.font = ContentPool.Instance.get_font('basic', 14)
        except:
            ContentPool.Instance.load_font('basic', 'front/test_py/fonts/Montserrat-Thin.ttf')
            self.font = ContentPool.Instance.get_font('basic', 14)

        all_container = HorContainer()
        self.container = all_container
        left = VerContainer()
        self.player_widgets = [
            PlayerFullWidget(self, 0),
            PlayerFullWidget(self, 1),
        ]
        left.add_widget(SPACE_FILLER)
        for player in self.player_widgets:
            left.add_widget(player)


        all_container.add_widget(left)
        all_container.add_widget(SPACE_FILLER)

        # set up player data
        # self.init_player_data_ui()

        # set up all other data data
        # self.init_other_data_ui()

    # def init_player_data_ui(self):
    #     container = VerContainer()
    #     self.top_player = PlayerContainer(self)
    #     self.bottom_player = PlayerContainer(self)
    #     self.hand = HandContainer(self)

    #     self.container.add_widget(container)

    #     mid_container = HorContainer()
    #     self.mid_label = LabelWidget(self.font, ' ')
    #     mid_container.add_widget(self.mid_label)
    #     mid_container.add_widget(SPACE_FILLER)

    #     sep = RectWidget(WHITE, max_height=10)
    #     container.add_widget(self.top_player)
    #     container.add_widget(sep)
    #     container.add_widget(self.bottom_player)
    #     container.add_widget(mid_container)
    #     container.add_widget(self.hand)
    #     container.add_widget(sep)

    # def init_other_data_ui(self):
    #     container = VerContainer()
    #     container.add_widget(SPACE_FILLER)
        
    #     self.last_played_card_container = HorContainer()
    #     self.last_played_card = CardWidget(self, [])
    #     self.last_played_card_container.add_widget(SPACE_FILLER)
    #     self.last_played_card_container.add_widget(self.last_played_card)
    #     self.last_played_card_container.add_widget(SPACE_FILLER)
    #     container.add_widget(LabelWidget(self.font, 'Last played:'))
    #     self.last_played_label = LabelWidget(self.font, '')
    #     container.add_widget(self.last_played_card_container)
    #     container.add_widget(self.last_played_label)
    #     container.add_widget(SPACE_FILLER)
    #     self.logs_container = LogsContainer(self)
    #     container.add_widget(self.logs_container)
    #     container.add_widget(SPACE_FILLER)

    #     self.container.add_widget(container)

    def draw(self):
        if self.last_state is None:
            return
        
        # self.coord_dict = {}
        super().draw()
        # if self.last_state.sourceID in self.coord_dict:
        #     coord = self.coord_dict[self.last_state.sourceID]
        #     util.draw_arrow(self.screen, (coord[0] + CARD_WIDTH/2, coord[1] + CARD_HEIGHT/2), pg.mouse.get_pos(), 3)
            
        # if self.last_state.cursorCard is not None:
        #     mx, my = pg.mouse.get_pos()
        #     w = self.cursor_card_widget
        #     w._draw(self.screen, Rect(mx - CARD_WIDTH/2, my - CARD_HEIGHT/2, CARD_WIDTH, CARD_HEIGHT), self.configs)

    def update(self):
        super().update()
        statej = self.read_msg()
        if statej != '':
            print(statej)
            parsed = parse_state(statej)
            self.load(parsed)
        # if self.fuze == 1:
        #     return
        # state = test_state()
        # self.load(state)
        # self.fuze = 1
    # fuze = 0

    def load(self, state):
        self.last_state = state
        for i in range(len(state.players)):
            self.player_widgets[i].load(state)
        # self.top_player.load(state.players[1-state.myData.playerI])
        # self.bottom_player.load(state.players[state.myData.playerI])
        # self.top_player.load(state.players[1])
        # self.bottom_player.load(state.players[0])
        # self.hand.load(state.myData.hand)

        # # print(state.newLogs)

        # self.mid_label.set_text(f'({state.request}) {state.prompt}')

        # if state.lastPlayed is not None:
        #     self.last_played_card.load(state.lastPlayed.card)
        #     self.last_played_label.set_text(f'(player: {state.lastPlayed.playerName})')

        # self.logs_container.load(state.newLogs)

        # self.cursor_card_widget.load(state.cursorCard)
  
    def send_response(self, response: str):
        self.send_msg(response)

    fuze = 1
    def read_msg(self):
        # if ClientWindow.fuze:
        #     result = open('state.json', 'r').read()
        #     ClientWindow.fuze = 0
        #     return result
        # return ''
    
        message = ''
        try :
            message_length_bytes = self.sock.recv(4)
            # print(message_length_bytes)
            message_length = int.from_bytes(message_length_bytes, byteorder='little')
            # print(f'Message length: {message_length}')

            # Receive the message itself
            while len(message) < message_length:
                message_bytes = self.sock.recv(message_length)
                message += message_bytes.decode('utf-8')

            # print('Read: ' + message)
        except socket.timeout:
            message = ''

        return message
    
    def send_msg(self, msg: str):
        message_length = len(msg)
        message_length_bytes = message_length.to_bytes(4, byteorder='little')
        message_bytes = msg.encode('utf-8')
        message_with_length = message_length_bytes + message_bytes
        self.sock.sendall(message_with_length)

    def process_key(self, event: pg.event.Event):
        super().process_key(event)

        if self.last_state is None:
            return
        
        if self.last_state.request == 'action' and event.key == pg.K_SPACE:
            self.send_response('pass')
            return