from front.dev.frame import *
from front.dev.window import *

class PlayerFullWidget(VerContainer):
    def __init__(self, window: 'ClientWindow', player_i: int, outline_color: tuple[int, int, int] = None):
        super().__init__(outline_color)
        self.player_i = player_i
        self.window = window

        self.init_ui()

    def init_ui(self):
        self.name_label = LabelWidget(self.window.font, '')
        self.deck_count_label = LabelWidget(self.window.font, '')
        self.discard_button = ButtonWidget(self.window.font, 'Discard', max_width=100, max_height=40)
        self.discard_button.click = self.discard_button_click

        self.add_widget(self.name_label)
        self.add_widget(self.deck_count_label)
        self.add_widget(self.discard_button)

    def load(self, data):
        player = data.players[self.player_i]
        name_text = player.name
        if data.myData.id == player.id:
            name_text += ' <you>'

        self.name_label.set_text(name_text)

        self.deck_count_label.set_text(f'Deck count: {player.deckCount}')
        return super().load(data)
    
    def discard_button_click(self):
        pass