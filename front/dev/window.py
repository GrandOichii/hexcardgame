import socket
import json
from types import SimpleNamespace

import pygame as pg

from front.dev.frame import *
from front.dev.frame import BLACK, WHITE, ClickConfig, Rect, WindowConfigs
from front.dev.widgets import *


def parse_state(textj):
    return json.loads(textj, object_hook=lambda d: SimpleNamespace(**d))


def warp_text(text: str, max_width: int, font: pg.font.Font) -> list[str]:
    words = [word.split(' ') for word in text.splitlines()]  # 2D array where each row is a list of words.
    space = font.size(' ')[0]  # The width of a space.
    pos = (0, 0)
    x, y = pos
    result = []
    rl = ''
    for line in words:
        for word in line:
            word_width, word_height = font.size(word)
            if x + word_width >= max_width:
                x = pos[0]  # Reset the x.
                y += word_height  # Start on new row.
                result += [rl]
                rl = ''
            rl += word + ' '
            x += word_width + space
        x = pos[0]  # Reset the x.
        y += word_height  # Start on new row.
    result += [rl]
    return result


class LogPart:
    def __init__(self, text='', cardRef='') -> None:
        self.text = text
        self.cardRef = cardRef


class LogWord(LabelWidget):
    def __init__(self, window: 'ClientWindow', log_part: LogPart, max_width: int):
        self.log_part = log_part

        text = log_part.text
        super().__init__(window.font, text, BLACK if log_part.cardRef == '' else RED, max_width=max_width)
    

def wrap_log(text: list[LogPart], max_width: int, font: pg.font.Font) -> list[list[LogPart]]:
    # words = [word.split(' ') for word in text.splitlines()]  # 2D array where each row is a list of words.
    words: list[list[LogPart]] = []
    for part in text:
        split = part.text.split(' ')
        for s in split:
            lp = LogPart(s, part.cardRef)
            words += [lp]
    words.insert(0, LogPart('- ', ''))

    space = font.size(' ')[0]  # The width of a space.
    pos = (0, 0)
    x, y = pos
    result = []
    rl = []
    # for line in words:
    for part in words:
        word_width, word_height = font.size(part.text)
        if x + word_width >= max_width:
            x = pos[0]  # Reset the x.
            y += word_height  # Start on new row.
            result += [rl]
            rl = []
        rl += [part, LogPart(' ', '')]
        x += word_width + space
    # x = pos[0]  # Reset the x.
    # y += word_height  # Start on new row.
    result += [rl]
    return result


class LogChunk(HorContainer):
    def __init__(self, window: 'ClientWindow', log_words: list[LogPart]):
        super().__init__()
        self.window = window
        mh = 0
        w = []
        for log in log_words:
            mw, h = window.font.size(log.text)
            if h > mh:
                mh = h

            l = LogWord(window, log, mw)
            w += [l]
        for ww in w:
            ww.max_height = mh
            self.add_widget(ww)
        self.add_widget(RectWidget(max_height=mh))
        # self.add_widget(SPACE_FILLER)
            # return

    def wrap(window: 'ClientWindow', log: list, max_width: int) -> list['LogChunk']:
        result = []

        m = '- '
        for p in log:
            m += p.text

        font = window.font
        lines = wrap_log(log, max_width, font)
        for line in lines:
            label = LogChunk(window, line)
            result += [label]

        return result
    
    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs):
        return super().draw(surface, bounds, configs)
    

class LogWidget(ScrollWidget):
    def __init__(self, window: 'ClientWindow', max_message_width: int):
        super().__init__(max_width=max_message_width)
        self.mmw = max_message_width

        self.widget = VerContainer()
        self.window = window

        self.acc_height = 0
        self.last_height = 0

    def _add_widget(self, widget):
        pass

    def add_logs(self, logs: list):
        for log in logs:
            
            # for lp in log:
            #     message += lp.text + ' '
            log_chunks = LogChunk.wrap(self.window, log, self.mmw)
            for chunk in log_chunks:
                self.widget.add_widget(chunk)
                self.acc_height += chunk.get_pref_height()

        if self.last_height == 0:
            return
        if self.last_height < self.acc_height:
            # print(self.last_height, self.acc_height)
            # print('amogus')
            self.scroll =  self.last_height - self.acc_height

    def draw(self, surface: pg.Surface, bounds: Rect, configs: WindowConfigs) -> tuple[int, int]:
        w, h = super().draw(surface, bounds, configs)
        self.last_height = h
        return w, h


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
        self.sock = None

    def on_close(self):
        super().on_close()
        if self.sock:
            self.sock.close()

    def config_connection(self):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.connect((self.host, self.port))

        self.sconfig = parse_state(self.read_msg())

        self.player_count = self.sconfig.playerCount
        self.player_i = self.sconfig.myI
        for player_w in self.player_widgets:
            player_w.player_i += self.player_i + 1
            if player_w.player_i >= self.player_count:
                player_w.player_i -= self.player_count
        self.sock.settimeout(.01)

    def init_ui(self):
        # TODO remove, for easier debugging
        self.font = None
        try:
            ContentPool.Instance.load_font('basic', 'fonts/Montserrat-Thin.ttf')
            self.font = ContentPool.Instance.get_font('basic', 18)
        except:
            ContentPool.Instance.load_font('basic', 'front/test_py/fonts/Montserrat-Thin.ttf')
            self.font = ContentPool.Instance.get_font('basic', 18)

        all_container = HorContainer()
        self.container = all_container

        # player info
        left = VerContainer()
        # TODO ocnfigure player count using received match info
        self.player_widgets = [
            PlayerFullWidget(self, 0),
            PlayerFullWidget(self, 1),
        ]
        left.add_widget(SPACE_FILLER)
        for player in self.player_widgets:
            left.add_widget(player)

        # logs
        right = VerContainer()

        self.log_widget = LogWidget(self, 200)

        right.add_widget(self.log_widget)
        # right.add_widget(RectWidget(max_height=0, max_width=50))
        # right.add_widget(SPACE_FILLER)

        all_container.add_widget(left)
        all_container.add_widget(SPACE_FILLER)
        all_container.add_widget(right)

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
            parsed = parse_state(statej)
            self.load(parsed)

    def load(self, state):
        self.last_state = state
        for i in range(len(state.players)):
            self.player_widgets[i].load(state)
        self.log_widget.add_logs(state.newLogs)
  
    def send_response(self, response: str):
        self.send_msg(response)

    fuze = 0
    def read_msg(self):
        if ClientWindow.fuze == 0:
            result = open('state.json', 'r').read()
            ClientWindow.fuze = 5000
            return result
        # ClientWindow.fuze -= 1
        return ''
    
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