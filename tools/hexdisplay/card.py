import curses
from tools.hexdisplay.util import *
import textwrap


class Card:
    def __init__(self, name:str='', cost:int=0, type:str='', text:str='', power:int=-1, life:int=0, max_movement:int=0):
        self.name = name
        self.cost = cost
        self.type = type
        self.text = text

        self.power: int = power
        self.life: int = life
        self.max_movement = max_movement
        self.movement = 0


class Placeable(Card):
    def __init__(self, label:str='', power:int=-1, life:int=0, name:str='', cost:int=0, type:str='', text:str='', owner_i: int=0, max_movement: int=0):
        super().__init__(name, cost, type, text, power, life, max_movement)
        self.label: str = label
        self.owner_i = owner_i
        
    def copy(self, owner_i: int) -> 'Placeable':
        return Placeable(self.label, self.power, self.life, self.name, self.cost, self.type, self.text, owner_i, self.max_movement)


# TODO add cycling entities on space pressing
MANA_DRILL_C = Placeable('MD', -1, 3, 'Mana Drill', 3, 'Structure', '{Unrestricted}\nGenerates 1 energy per turn.')
ENTITIES = [
    Placeable('Ca', -1, 10, 'Castle', 10, 'Structure', ''),
    Placeable('Fo', -1, 3, 'Fort', 3, 'Structure', 'Your n. tiles have +1 defence.'),
    MANA_DRILL_C,

    Placeable('HS', 2, 2, 'Hidden Spy', 4, 'Unit - Rogue', '{Hidden}', max_movement=2),
    # Placeable('MI', 1, 3, 'Mage Initiate', 2, 'Unit - Mage', '', max_movement=1),
    # Placeable('WI', 2, 2, 'Warrior Initiate', 2, 'Unit - Warrior', '', max_movement=1),
    # Placeable('RI', 1, 2, 'Rogue Initiate', 2, 'Unit - Rogue', '', max_movement=2),
    Placeable('CM', 2, 2, 'Combat Medic', 3, 'Unit - Warrior', 'At the start of your turn, [CARDNAME] restores 1 life to all your n. Units.', max_movement=1),
    Placeable('HA', 2, 2, 'Hired Assassin', 3, 'Unit - Rogue', 'Destroys Unit on attack.', max_movement=2),
    # Placeable('EG', 2, 2, 'Elven General', 4, 'Unit - Warrior', 'Every 3 turns, summon an [Elf] into a n. tile.', max_movement=1),
    # Placeable('El', 1, 1, 'Elf', 1, 'Unit', '', max_movement=1),
    Placeable('BN', 1, 2, 'Baar Swamp Necromancer', 3, 'Unit - Mage', 'At the start of your turn, revive a n. grave into a [Zombie].', max_movement=1),
    Placeable('Zo', 2, 2, 'Zombie', 2, 'Unit', 'Leaves no grave.', max_movement=1),
    Placeable('UR', 3, 2, 'Urakshi Raider', 3, 'Unit - Warrior', '{Fast}', max_movement=1),
    Placeable('US', 2, 2, 'Urakshi Shaman', 3, 'Unit - Mage', 'Spell cast by [CARDNAME] do +1 damage.', max_movement=1),
    Placeable('UT', 1, 1, 'Urakshi Thief', 3, 'Unit - Rogue', '{Fast}\n{Hidden}', max_movement=2),
    Placeable('Br', 6, 5, 'Brute', 5, 'Unit', '', max_movement=1),
    Placeable('VD', 7, 7, 'Volcanic Dragon', 7, 'Unit', 'At the start of turn, deal 1 damage to all n. enemy Structures/Units.', max_movement=1),
]


CARDS = [
    Card('Flame Eruption', 3, 'Spell', 'Deal 2 damage to all n. Structures/Units.'),
    Card('Spread Influence', 4, 'Spell', 'Gain control of the current and all n. neutral tiles.'),
    Card('Troop Warp', 5, 'Spell', 'Teleport any one of your Warriors into a n. tile.'),
] + ENTITIES


# TODO not only entities
def card_by_name(name: str):
    for card in CARDS:
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
        # name
        win.addstr(y + 1, x + 1, textwrap.shorten(self.card.name, CARD_WIDTH-3, placeholder='...'))
        # cost
        cs = str(self.card.cost)
        win.addstr(y + 1, x + CARD_WIDTH - len(cs) - 1, cs)
        # type
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
        