import curses
from tools.hexdisplay.util import *
import textwrap


class Card:
    def __init__(self, name:str='', cost:int=0, type:str='', text:str='', power:int=-1, life:int=0):
        self.name = name
        self.cost = cost
        self.type = type
        self.text = text

        self.power: int = power
        self.life: int = life


class Placeable(Card):
    def __init__(self, label:str='', power:int=-1, life:int=0, name:str='', cost:int=0, type:str='', text:str=''):
        super().__init__(name, cost, type, text, power, life)
        self.label: str = label
        

    def copy(self) -> 'Placeable':
        return Placeable(self.label, self.power, self.life, self.name, self.cost, self.type, self.text)


# TODO add cycling entities on space pressing
MANA_DRILL_C = Placeable('MD', -1, 5, 'Mana Drill', 3, 'Structure', '{Unrestricted}\nGenerates 1 energy per turn.')
ENTITIES = [
    Placeable('Ca', -1, 10, 'Castle', 10, 'Structure', ''),
    Placeable('Fo', -1, 5, 'Fort', 3, 'Structure', 'Your n. tiles have +1 defence.'),
    MANA_DRILL_C,

    Placeable('HS', 2, 2, 'Hidden Spy', 4, 'Unit - Rogue', '{Hidden}'),
    Placeable('MI', 1, 3, 'Mage Initiate', 2, 'Unit - Mage', ''),
    Placeable('WI', 2, 2, 'Warrior Initiate', 2, 'Unit - Warrior', ''),
    Placeable('RI', 1, 2, 'Rogue Initiate', 2, 'Unit - Rogue', ''),
    Placeable('CM', 2, 2, 'Combat Medic', 3, 'Unit - Warrior', 'At the start of your turn, [CARDNAME] restores 1 life to all your n. Units.'),
    Placeable('HA', 2, 2, 'Hired Assassin', 3, 'Unit - Rogue', 'Destroys Unit on attack.'),
    Placeable('EG', 2, 2, 'Elven General', 4, 'Unit - Warrior', 'Every 3 turns, summon an [Elf] into a n. tile.'),
    Placeable('El', 1, 1, 'Elf', 1, 'Unit', ''),
    Placeable('BN', 1, 2, 'Baar Swamp Necromancer', 3, 'Unit - Mage', 'At the start of your turn, revive a n. grave into a [Zombie].'),
    Placeable('Zo', 2, 2, 'Zombie', 2, 'Unit', ''),
    Placeable('UR', 3, 2, 'Urakshi Raider', 3, 'Unit - Warrior', '{Fast}'),
    Placeable('US', 2, 2, 'Urakshi Shaman', 3, 'Unit - Mage', 'Spell cast by [CARDNAME] do +1 damage.'),
    Placeable('UT', 1, 1, 'Urakshi Thief', 3, 'Unit - Rogue', '{Fast}\n{Hidden}'),
    Placeable('Br', 6, 5, 'Brute', 5, 'Unit', ''),
]


# TODO not only entities
def card_by_name(name: str):
    for card in ENTITIES:
        if card.name == name:
            return card
    return None


def entity_by_name(name: str):
    for card in ENTITIES:
        if card.name == name:
            return card
    return None


CARD_HEIGHT = 15
CARD_WIDTH = 20
class CardSprite:
    def __init__(self, card: Card=None):
        # TODO
        self.load(card)

    def load(self, card: Card):
        self.card = card

    def draw(self, win: curses.window, y: int, x: int):
        # name box
        box(win, y, x, 3, CARD_WIDTH, continue_down=True)
        # type box
        box(win, y + 2, x, 3, CARD_WIDTH, True, True)
        # text
        box(win, y + 4, x, CARD_HEIGHT - 4, CARD_WIDTH, True)
        
        if not self.card: return
        win.addstr(y + 1, x + 1, textwrap.shorten(self.card.name, CARD_WIDTH-2, placeholder='...'))
        win.addstr(y + 3, x + 1, textwrap.shorten(self.card.type, CARD_WIDTH-2, placeholder='...'))
        lines = []
        for line in self.card.text.split('\n'):
            lines += textwrap.wrap(line, CARD_WIDTH-2)
        # TODO shorten text if too many lines
        for i in range(len(lines)):
            win.addstr(y + 5 + i, x + 1, lines[i])
        
        if self.card.power > 0:
            win.addstr(y + CARD_HEIGHT - 2, x + 1, str(self.card.power))
        
        if self.card.life >= 0:
            s = str(self.card.life)
            win.addstr(y + CARD_HEIGHT - 2, x + CARD_WIDTH - len(s) - 1, s)
        