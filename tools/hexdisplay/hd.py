import curses
import re
from tools.hexdisplay.card import *

from tools.hexdisplay.list import ListTemplate

ISLAND_LAYERS = [
    1,
    2,
    3,
    4,
    3,
    4,
    3,
    4,
    3,
    4,
    3,
    4,
    3,
    4,
    3,
    2,
    1
]


TILE_SPRITE = [
    '  ---  ',
    '/     \\',
    '       ',
    '\\     /',
    '  ---  ',
]


class Map:
    def __init__(self):
        self.generate_tiles()

    def generate_tiles(self):
        self.tiles: list[list[Tile]] = []
        m = max(ISLAND_LAYERS)
        for y in range(len(ISLAND_LAYERS)):
        # for layer in ISLAND_LAYERS:
            layer = ISLAND_LAYERS[y]
            offset = (m - layer)//2
            a = []
            for i in range(m):
                tile = None
                if not (i < offset or i >= offset + layer):
                # TODO remove
                # if True:
                    tile = Tile()
                    tile.xpos = i
                    tile.ypos = y
                a += [tile]
            self.tiles += [a]


# DECKLIST = 4 * [card.copy() for card in ENTITIES if card.name != 'Mana Drill']
# mdc = int(len(DECKLIST)*35/65)
# DECKLIST += [MANA_DRILL_C.copy() for i in range(mdc)]


class Tile:
    def __init__(self):
        self.xpos = -1
        self.ypos = -1

        self.owner_id = 0
        self.entity: Placeable = None
        self.entity_i: int = -1


class DrawLayer:
    def __init__(self, game: 'Game'):
        self.game = game

    def draw(self, game: 'Game'):
        pass


class TileDrawLayer(DrawLayer):
    def __init__(self, game: 'Game'):
        super().__init__(game)

    def basic_draw_sprite(self, win: curses.window, i, j, tile: Tile):
        # y = 0
        y = (len(TILE_SPRITE) - 3)*i
        x = 12 * j
        if i % 2 == 0:
            x += 6
        for ii in range(len(TILE_SPRITE)):
            win.addstr(y + ii, x, TILE_SPRITE[ii])
        en = tile.entity

        # draw location
        if self.game.location_toggled:
            win.addstr(y + 3, x + 2, f' . ')
            win.addstr(y + 3, x + 1, str(i))
            js = str(j)
            win.addstr(y + 3, x + 6 - len(js), js)

        if not en: return

        # draw entity info
        win.addstr(y + 1, x + 2, en.label)
        if en.power > 0:
            win.addstr(y + 2, x + 1, str(en.power))
        if en.life > 0:
            ls = str(en.life)
            win.addstr(y + 2, x + 6 - len(ls), ls)

    def draw_sprite(self, win, y, x, tile):
        if not tile:
            return
        
        self.basic_draw_sprite(win, y, x, tile)

    
# TODO fix this mess
class DrawMapLayer(TileDrawLayer):
    def __init__(self, game: 'Game') -> None:
        super().__init__(game)

    def draw(self):
        self.game.win.attron(curses.color_pair(1))
        tiles = self.game.map.tiles
        height = len(tiles)
        width = len(tiles[0])
        for i in range(height):
            for j in range(width):
                self.draw_sprite(self.game.win, i, j, tiles[i][j])

        self.game.win.attroff(curses.color_pair(1))


class DrawSelectedTileLayer(TileDrawLayer):
    def __init__(self, game: 'Game'):
        super().__init__(game)

    def draw(self):
        i, j = self.game.mouse_pos
        self.game.win.attron(curses.color_pair(2))
        self.basic_draw_sprite(self.game.win, i, j, self.game.map.tiles[i][j])
        self.game.win.attroff(curses.color_pair(2))


PLAYER_COLOR_PAIR_MAP = {
    1: 3,
    2: 4
}


class DrawOwnedTiles(TileDrawLayer):
    def __init__(self, game: 'Game'):
        super().__init__(game)
    
    def draw(self):
        tiles = self.game.map.tiles
        height = len(tiles)
        width = len(tiles[0])

        for i in range(height):
            for j in range(width):
                if tiles[i][j] is None or tiles[i][j].owner_id == 0:
                    continue
                p = PLAYER_COLOR_PAIR_MAP[tiles[i][j].owner_id]
                self.game.win.attron(curses.color_pair(p))
                self.draw_sprite(self.game.win, i, j, tiles[i][j])
                self.game.win.attron(curses.color_pair(p))


class DrawSelectedCardLayer(DrawLayer):
    def __init__(self, game: 'Game'):
        super().__init__(game)
        self.card_sprite = CardSprite()

    def draw(self):
        tile = self.game.selected_tile()
        en = tile.entity
        self.card_sprite.load(en)
        self.game.win.addstr(1, 45, 'Selected entity:')
        self.card_sprite.draw(self.game.win, 2, 45)


class DrawSelectedTileInfoLayer(DrawLayer):
    def __init__(self, game: 'Game'):
        super().__init__(game)

    def draw(self):
        win = self.game.win
        x = 45 + CARD_WIDTH + 1
        tile = self.game.selected_tile()
        win.addstr(1, x, 'Selected tile info:')
        win.addstr(2, x, f'Pos: {tile.ypos} -- {tile.xpos}')


PLAYER_AREA_HEIGHT = 25
PLAYER_AREA_WIDTH = 40
class DrawPlayersLayer(DrawLayer):
    def __init__(self, game: 'Game', player: 'PlayerData'):
        super().__init__(game)
        self.player = player

        self.list = ListTemplate(self.game.win, [card.name for card in ENTITIES], PLAYER_AREA_HEIGHT - CARD_HEIGHT - 2)
        self.card_sprite = CardSprite()

    def draw(self):
        selected = self.player.player_i == self.game.cur_player_i
        color = curses.color_pair(PLAYER_COLOR_PAIR_MAP[self.player.player_i+1])
        win = self.game.win
        # border
        y = 3 + CARD_HEIGHT
        x = 45 + self.player.player_i*PLAYER_AREA_WIDTH
        box(win, y, x, PLAYER_AREA_HEIGHT, PLAYER_AREA_WIDTH, attr=color)
        if selected:
            win.addstr(y, x + 1, '>  <')
        win.addstr(y, x + 2, f'P{self.player.player_i+1}', color)

        cname = self.list.selected()
        if not cname: return

        # draw selected card
        card = card_by_name(cname)

        self.card_sprite.load(card)
        self.card_sprite.draw(self.game.win, y + 1, x + 1)
        self.list.draw(y + 1 + CARD_HEIGHT, x + 1, selected)


class DrawLastErrorLayer(DrawLayer):
    def __init__(self, game: 'Game') -> None:
        super().__init__(game)

    def draw(self):
        y = self.game.height - 1
        x = 1
        self.game.win.addstr(y, x, self.game.last_error, curses.color_pair(10))


class PlayerData:
    def __init__(self, player_i: int):
        self.player_i = player_i

POINT_RANGE_REGEX = re.compile('\[([0-9]+)\:([0-9]+)\]')
class Command:
    def __init__(self, game: 'Game'):
        self.game = game

    def execute(self, cname: str, args: list[str]) -> str:
        return 'Command not implemented'

    def get_tile(self, point: tuple[int, int]) -> tuple[Tile, str]:
        if point[0] > len(self.game.map.tiles):
            return None, f'Y point {point[0]} is out of bounds (max: {len(self.game.map.tiles)})'
        if point[1] > len(self.game.map.tiles[0]):
            return None, f'X point {point[1]} is out of bounds (max: {len(self.game.map.tiles[0])})'
        tile = self.game.map.tiles[point[0]][point[1]]
        if not tile:
            ps = f'{point[0]}.{point[1]}'
            return None, f'Tile at {ps} is empty'
        return tile, ''
    
    def parse_point(self, s: str) -> tuple[tuple[int, int], str]:
        points, err = self.parse_points(s)
        if not points: return None, err
        if len(points) > 1: return None, f'Can\'t get more than 1 point in {s}'
        return points[0], ''

    def parse_points(self, s: str) -> tuple[list[tuple[int, int]], str]:
        if s == '.':
            return (self.game.mouse_pos[0], self.game.mouse_pos[1]), ''
        points = s.split('.')
        err = f'Can\'t parse point {s}'
        if len(points) != 2:
            return None, err
        try:
            result = []
            ys = points[0]
            xs = points[1]

            yrange, err = self.extract_range(ys)
            if not yrange:
                return None, err
            xrange, err = self.extract_range(xs)
            if not xrange:
                return None, err
            
            for i in range(yrange[0], yrange[1]+1):
                for j in range(xrange[0], xrange[1]+1):
                    result += [(i, j)]

            # check for ranges
            # y = int(points[0])
            # x = int(points[1])
            return result, ''
        except:
            return None, err

    def extract_range(self, s: str) -> tuple[tuple[int, int], str]:
        if ':' in s:
            m = re.match(POINT_RANGE_REGEX, s)
            g = m.groups()
            if len(g) != 2:
                return None, f'Can\'t parse range {s}'
            return (int(g[0]), int(g[1])), ''
        v = int(s)
        return (v, v), ''

class PutCommand(Command):
    def __init__(self, game: 'Game'):
        super().__init__(game)

    def execute(self, cname: str, args: list[str]) -> str:
        if len(args) < 2:
            return f'Incorrect number of arguments for <{cname}> command'
        point_raw = args[0]
        points, err = self.parse_points(point_raw)
        ename = ' '.join(args[1:])
        entity = entity_by_name(ename)
        if not entity:
            return f'No entity with name "{ename}"'
        for point in points:
            if not point:
                return err
            tile, err = self.get_tile(point)
            if not tile:
                return err
            tile.entity = entity.copy()
        return ''
    

class ToggleLocationCommand(Command):
    def __init__(self, game: 'Game'):
        super().__init__(game)

    def execute(self, cname: str, args: list[str]):
        if len(args) != 0:
            return f'Too many args for <{cname}> command'
        self.game.location_toggled = not self.game.location_toggled
        return ''


class MoveEntityCommand(Command):
    def __init__(self, game: 'Game'):
        super().__init__(game)

    def execute(self, cname: str, args: list[str]) -> str:
        if len(args) != 2:
            return f'Incorrect number of arguments for <{cname}> command'
        p1, err = self.parse_point(args[0])
        if not p1:
            return err
        p2, err = self.parse_point(args[1])
        if not p2:
            return err
        tile1, err = self.get_tile(p1)
        if not tile1:
            return err
        tile2, err = self.get_tile(p2)
        if not tile2:
            return err
        en = tile1.entity
        if en is None:
            return f'No entity in tile at <{args[0]}>'
        tile1.entity = None
        tile2.entity = en
        return ''


class TileOwnerSetCommand(Command):
    def __init__(self, game: 'Game'):
        super().__init__(game)

    def execute(self, cname: str, args: list[str]) -> str:
        if len(args) != 2:
            return f'Wrong number of arguments for <{cname}> command'
        points, err = self.parse_points(args[0])
        if not points:
            return err
        for point in points:
            tile, err = self.get_tile(point)
            if not tile:
                continue
                # return err
            try:
                pi = int(args[1])
                if pi > 3:
                    return f'Can\'t set owner id to {pi}'
                tile.owner_id = pi
            except:
                return f'Failed to parse player index {args[1]}'
        return ''


class CommandMaster:
    def __init__(self, game: 'Game'):
        self.game = game

        self.command_map = {
            'put': PutCommand(game),
            'moveen': MoveEntityCommand(game),
            'toggleloc': ToggleLocationCommand(game),
            'tos': TileOwnerSetCommand(game)
        }

    def execute(self, command: str) -> str:
        split = command.split(' ')
        operator = split[0]
        args = split[1:]
        if not operator in self.command_map:
            return f'Command <{operator}> not found'
        
        c = self.command_map[operator]
        return c.execute(operator, args)


class Game:
    def __init__(self, stdscr: curses.window):
        self.location_toggled = False
        self.command_master = CommandMaster(self)

        self.win = stdscr
        self.height, self.width = self.win.getmaxyx()
        self.entity_movement_mode = False
        self.held_entity: Placeable = None
        self.last_error: str = ''

        # player data
        self.cur_player_i: int = 0
        self.players: list[PlayerData] = [
            PlayerData(0),
            PlayerData(1)
        ]

        # tile colors
        curses.init_pair(1, curses.COLOR_WHITE, -1)
        curses.init_pair(2, curses.COLOR_RED, -1)
        # player colors
        curses.init_pair(3, curses.COLOR_CYAN, -1)
        curses.init_pair(4, curses.COLOR_MAGENTA, -1)
        # error color
        curses.init_pair(10, curses.COLOR_RED, -1)

        self.player_containers: list[DrawPlayersLayer] = [
            DrawPlayersLayer(self, self.players[0]),
            DrawPlayersLayer(self, self.players[1]),
        ]

        self.draw_layers: list[DrawLayer] = [
            DrawMapLayer(self),
            DrawOwnedTiles(self),
            DrawSelectedTileLayer(self),
            DrawSelectedTileInfoLayer(self),
            DrawSelectedCardLayer(self),
            DrawLastErrorLayer(self),
        ] + self.player_containers

        self.map = Map()
        self.mouse_pos = [len(self.map.tiles)//2, len(self.map.tiles[0])//2]

    def run(self):
        self.running = True
        while self.running:
            self.draw()
            self.input()
            self.win.refresh()
        # TODO end

    def draw(self):
        self.win.clear()
        for layer in self.draw_layers:
            layer.draw()

    def input(self):
        key = self.win.getch()

        # escape
        if key == 27:
            self.running = False
            return

        # command entering
        if key == ord('/'):
            self.win.addstr(self.height - 1, 0, ' ' * (self.width - 1))
            y = self.height - 1
            x = 1
            self.win.addstr(y, x, '>')
            curses.echo()
            curses.curs_set(1)
            command = self.win.getstr(y, x + 2).decode(encoding='utf-8')
            err = self.command_master.execute(command)
            curses.curs_set(0)
            curses.echo(False)
            self.last_error = err
            return

        # card in hand cycling keys
        if key == curses.KEY_DOWN:
            self.current_player_container().list.scroll_down()
            return
        if key == curses.KEY_UP:
            self.current_player_container().list.scroll_up()
            return

        # current player cycling keys
        if key == ord('\t'):
            self.cur_player_i = 1 - self.cur_player_i
            return

        # modify entity life keys
        if key == ord(']'):
            tile = self.selected_tile()
            en = tile.entity
            if not en: return

            en.life += 1
            return
        if key == ord('['):
            tile = self.selected_tile()
            en = tile.entity
            if not en: return

            en.life -= 1
            return
        
        # modify entity power keys
        if key == ord('}'):
            tile = self.selected_tile()
            en = tile.entity
            if not en: return

            en.power += 1
            return
        if key == ord('{'):
            tile = self.selected_tile()
            en = tile.entity
            if not en: return

            en.power -= 1
            return

        # player territory 
        if key == ord('0'):
            t = self.selected_tile()
            t.owner_id = 0
            return        
        if key == ord('1'):
            t = self.selected_tile()
            t.owner_id = 1
            return        
        if key == ord('2'):
            t = self.selected_tile()
            t.owner_id = 2
            return    

        prev = self.selected_tile()
        def check_entity_movement():
            if self.entity_movement_mode:
                self.selected_tile().entity = prev.entity
                prev.entity = None

        # directional keys
        if key == ord('w'):
            new_pos = self.mouse_pos[0] - 2
            if new_pos >= 0 and self.map.tiles[new_pos][self.mouse_pos[1]]:
                self.mouse_pos[0] = new_pos
                check_entity_movement()
            return
        if key == ord('s'):
            new_pos = self.mouse_pos[0] + 2
            if new_pos < len(self.map.tiles) and self.map.tiles[new_pos][self.mouse_pos[1]]:
                self.mouse_pos[0] = new_pos
                check_entity_movement()
            return

        if key == ord('q'):
            new_pos_y = self.mouse_pos[0] - 1
            new_pos_x = self.mouse_pos[1]
            if new_pos_y % 2 == 0:
                new_pos_x -= 1
            if new_pos_y < 0 or new_pos_x < 0 or self.map.tiles[new_pos_y][new_pos_x] is None:
                return
            self.mouse_pos[0] = new_pos_y
            self.mouse_pos[1] = new_pos_x
            check_entity_movement()
            return
        if key == ord('d'):
            new_pos_y = self.mouse_pos[0] + 1
            new_pos_x = self.mouse_pos[1]
            if new_pos_y % 2 == 1:
                new_pos_x += 1
            if new_pos_y >= len(self.map.tiles) or new_pos_x >= len(self.map.tiles[0]) or self.map.tiles[new_pos_y][new_pos_x] is None:
                return
            self.mouse_pos[0] = new_pos_y
            self.mouse_pos[1] = new_pos_x
            check_entity_movement()
            return
        if key == ord('a'):
            new_pos_y = self.mouse_pos[0] + 1
            new_pos_x = self.mouse_pos[1]
            if new_pos_y % 2 == 0:
                new_pos_x -= 1
            if new_pos_y >= len(self.map.tiles) or new_pos_x < 0 or self.map.tiles[new_pos_y][new_pos_x] is None:
                return
            self.mouse_pos[0] = new_pos_y
            self.mouse_pos[1] = new_pos_x
            check_entity_movement()
            return
        if key == ord('e'):
            new_pos_y = self.mouse_pos[0] - 1
            new_pos_x = self.mouse_pos[1]
            if new_pos_y % 2 == 1:
                new_pos_x += 1
            if new_pos_y < 0 or new_pos_x >= len(self.map.tiles[0]) or self.map.tiles[new_pos_y][new_pos_x] is None:
                return
            self.mouse_pos[0] = new_pos_y
            self.mouse_pos[1] = new_pos_x
            check_entity_movement()
            return
        
        # entity cycling keys
        if key == ord('>'):
            tile = self.selected_tile()
            tile.entity_i += 1
            if tile.entity_i >= len(ENTITIES):
                tile.entity_i = -1
                tile.entity = None
                return
            tile.entity = ENTITIES[tile.entity_i].copy()
            return
        if key == ord('<'):
            tile = self.selected_tile()
            tile.entity_i -= 1
            if tile.entity_i == -1:
                tile.entity = None
                return
            if tile.entity_i < -1:
                tile.entity_i = len(ENTITIES) - 1
            tile.entity = ENTITIES[tile.entity_i].copy()
            return
        
        # entity movement keys
        if key == ord('m'):
            self.entity_movement_mode = not self.entity_movement_mode
            return
        
    def selected_tile(self) -> Tile:
        return self.map.tiles[self.mouse_pos[0]][self.mouse_pos[1]]

    def current_player_container(self) -> DrawPlayersLayer:
        return self.player_containers[self.cur_player_i]


def main(stdscr: curses.window):
    curses.curs_set(0)
    curses.set_escdelay(1)
    curses.use_default_colors()

    g = Game(stdscr)
    g.run()

'''
  ---  
/     \
\     /
  ---
'''