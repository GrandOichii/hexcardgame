import pygame as pg
import shapely
import math

from front.dev1.tileset import *

BLACK = (0, 0, 0)
WHITE = (255, 255, 255)
RED = (255, 0, 0)
LRED = (255, 100, 100)
GREEN = (0, 255, 0)
LGREEN = (100, 255, 100)
BLUE = (0, 0, 255)
LBLUE = (100, 100, 255)
GRAY = (80, 80, 80)
LGRAY = (200, 200, 200)


class CardSprite:
    BaseSprite = None

    def __init__(self):
        if CardSprite.BaseSprite is None:
            self.create_sprites()

    def create_sprites(self):
        scale = 1
        border = 2
        width = 35*scale
        height = 45*scale
        CardSprite.BaseSprite = pg.Surface((width, height), pg.SRCALPHA)
        CardSprite.BaseSprite.fill(BLACK)
        fg = pg.Surface((width - 2 * border, height - 2 * border), pg.SRCALPHA)
        fg.fill(WHITE)
        CardSprite.BaseSprite.blit(fg, (border, border))


class TileSprite:
    Height = 128
    Width = 128
    
    BaseSprite = None
    ColorSprite = None
    SelectedSprite = None

    ColorOffset = 2
    HorOffset = None
    VerOffset = None

    Tileset = None

    def __init__(self, pos: tuple[int, int]):
        if TileSprite.BaseSprite is None:
            self.generate_sprite()
        if TileSprite.Tileset is None:
            self.load_tileset()
        self.pos = pos
        self.rect = TileSprite.BaseSprite.get_rect()
        # self.polygon = shapely.Polygon(self.get_points())

    def load_tileset(self):
        TileSprite.Tileset = Tileset('tilesets/tileset1.png')
        # tilemap = Tilemap(self.tileset)

    def create_polygon(self, size: tuple[int, int], color: tuple[int, int, int]=None) -> pg.Surface:
        result = pg.Surface((size), pg.SRCALPHA)
        h = size[0]
        w = size[1]
        if not color:
            color = (0, 0, 0, 0)
        #     result.fill(color)

        ver_offset = w / 2
        hor_offset = ver_offset * 0.43
        TileSprite.HorOffset = hor_offset
        TileSprite.VerOffset = ver_offset
        # pg.draw.polygon(result, color, [
        #     (hor_offset, 0),
        #     (w - hor_offset, 0),
        #     (w, h - ver_offset),
        #     (w - hor_offset, h),
        #     (hor_offset, h),
        #     (0, ver_offset)
        # ])
        pg.draw.polygon(result, color, self.get_points(0, 0, size))

        # empty = (0, 0, 0, 0)
        # pg.draw.polygon(result, empty, ((0, 0), (ver_offset, 0), (0, hor_offset)))
        # pg.draw.polygon(result, empty, ((0, h), (ver_offset, h), (0, h - hor_offset)))
        # pg.draw.polygon(result, empty, ((w, 0), (w, hor_offset), (w - ver_offset, 0)))
        # pg.draw.polygon(result, empty, ((w, h), (w - ver_offset, h), (w, h - hor_offset)))
        return result
    
    def get_points(self, x, y, size=None):
        if not size:
            size = (TileSprite.Width, TileSprite.Height)
        return [
            (x + TileSprite.HorOffset, y + 0),
            (x + size[0] - TileSprite.HorOffset, y + 0),
            (x + size[0], y + size[1] - TileSprite.VerOffset),
            (x + size[0] - TileSprite.HorOffset, y + size[1]),
            (x + TileSprite.HorOffset, y + size[1]),
            (x + 0, y + TileSprite.VerOffset)
        ]

    def generate_sprite(self):
        TileSprite.BaseSprite = self.create_polygon((TileSprite.Width, TileSprite.Height), BLACK)
        # TileSprite.SelectedSprite = self.create_polygon((TileSprite.Width, TileSprite.Height), RED)
        TileSprite.SelectedSprite = self.create_polygon((TileSprite.Width - TileSprite.ColorOffset, TileSprite.Height - TileSprite.ColorOffset), RED)
        TileSprite.ColorSprite = self.create_polygon((TileSprite.Width - TileSprite.ColorOffset, TileSprite.Height - TileSprite.ColorOffset), WHITE)

        # color = BLACK
        # pi2 = 2 * 3.14
        # radius = h / 2
        # position = (w // 2, h // 2)
        # n = 6
        # pg.draw.polygon(result, (0, 0, 0, 0), ((w, 0), (w - ver_offset, h), (w, h - hor_offset)))


        # for i in range(0, n):
        #     pg.draw.line(result, color, position, (math.cos(i / n * pi2) * radius + position[0], math.sin(i / n * pi2) * radius + position[1]))

        # pg.draw.lines(result,
        #     color,
        #     True,
        #     [(math.cos(i / n * pi2) * radius + position[0], math.sin(i / n * pi2) * radius + position[1]) for i in range(0, n)])
        # print()

    def draw(self, surface: pg.Surface):
        p = shapely.Polygon(self.get_points(self.rect.x, self.rect.y))
        m = pg.mouse.get_pos()
        in_mouse = p.contains(shapely.Point(m))

        bg = TileSprite.BaseSprite
        surface.blit(bg, self.rect)
        r = self.rect.copy()
        r.x += TileSprite.ColorOffset // 2
        r.y += TileSprite.ColorOffset // 2

        fg = TileSprite.ColorSprite
        if in_mouse:
            fg = TileSprite.SelectedSprite
        surface.blit(fg, r)
        if in_mouse:
            s = TileSprite.Tileset.tiles[2]
            size = s.get_size()
            r = (self.rect.x + (self.rect.width - size[0])/2, self.rect.y + (self.rect.height - size[1])/2, size[0], size[1])
            # r.x = self.rect.x
            # r.y = self.rect.y
            surface.blit(s, r)
            Window.SelectedTile = self


class Window:
    SelectedTile = None

    def __init__(self):
        self.num = 3

        pg.init()
        self.running = False
        self.clock = pg.time.Clock()
        self.fps = 60

        self.width, self.height = 800, 600

        self.screen = pg.display.set_mode((self.width, self.height))
        self.create_tiles()

        self.tile_size = 128

    def create_tiles(self):
        self.tiles: list[TileSprite] = []
        hor = 3
        ver = 8

        # dummy tile
        TileSprite((-1, -1))
        
        bw, bh = TileSprite.Height, TileSprite.Width
        for i in range(ver):
            for j in range(hor):
                y = (bh*0.5 - TileSprite.ColorOffset*0)*i
                # y = (bh*0.5)*i
                base = bw - TileSprite.HorOffset
                # base -= TileSprite.ColorOffset
                x = base * 2 * j + (1 - i % 2) * (base)
                
                # y = i * bh
                # x = j * bw

                t = TileSprite((j, i))
                t.rect.x = x
                t.rect.y = y
                self.tiles += [t]
        # self.sprite1 = TileSprite()
        # self.sprite1.rect.x = 100
        # self.sprite2 = TileSprite()
        # self.sprite2.rect.y = 200

    def run(self):
        self.running = True
        while self.running:
            # clock
            self.clock.tick(self.fps)

            self.update()

            # events
            self.events()

            # draw
            self.draw()

            # refresh screen
            pg.display.flip()
        self.on_close()

    def on_close(self):
        pass

    def update(self):
        pass

    def events(self):
        for event in pg.event.get():
            if event.type == pg.QUIT:
                self.running = False
                return
            if event.type == pg.MOUSEWHEEL:
                self.radius += event.y
                # self.x_offset += event.y
                # self.y_offset -= event.y
                self.y_offset = self.radius - 90
                # self.y_offset += event.y
                # print(self.radius)
                # self.offset_value += event.x
            if event.type == pg.KEYDOWN:
                if event.key == pg.K_q:
                    self.num += 1
                if event.key == pg.K_w:
                    self.num -= 1
                # if event.key == pg.K_RIGHT:
                #     self.x_offset += 10
                #     # print(event.key)
                #     # self.tile_size *= 2
                #     print(self.x_offset)
                #     return
                # if event.key == pg.K_LEFT:
                #     self.x_offset -= 10
                #     # self.tile_size /= 2
                #     print(self.x_offset)
                #     return
                # if event.key == pg.K_DOWN:
                #     self.y_offset += 10
                #     # print(event.key)
                #     # self.tile_size *= 2
                #     print(self.y_offset)
                #     return
                # if event.key == pg.K_UP:
                #     self.y_offset -= 10
                #     # self.tile_size /= 2
                #     print(self.y_offset)
                #     return
                # return

    def draw(self):
        Window.SelectedTile = None
        # self.sprite.Sprite.
        self.screen.fill(WHITE)
        # for tile in self.tiles:
        #     tile.rect.height = self.tile_size
        #     tile.rect.width = self.tile_size
        #     # print(self.tile_size)
        #     tile.draw(self.screen)


        # radius = self.radius
        # y = self.height + self.y_offset
        # x = radius + self.x_offset

        amount = self.num
        max_between = 10
        hand_distance = self.width * 3 / 4

        hand_angle = 180
        angle_between = hand_angle / amount
        angle = angle_between / 2
        # angle = math.pi / 4 * 3
        # angle = 45

        # for i in range(amount):
            

        CardSprite()
        sprite = CardSprite.BaseSprite
        sw, sh = sprite.get_size()

        start_y = self.height - 1
        start_x = 0

        d_between = min(max_between, hand_distance // (amount) - sw)
        space = sw * amount + d_between * (amount - 1)
        x = start_x + (hand_distance//2 - space//2)

        for i in range(amount):
            # r = (x + i * d_between, start_y, sw, sh)
            r = (x + i * (d_between + sw), start_y - sh, sw, sh)
            a = 90 - angle
            rotated = pygame.transform.rotate(sprite, a)
            # r = rotated.get_rect(center=rotated.get_rect().center)
            # r.x = x + i * (d_between + sw)
            # r.y = start_y - sh
            self.screen.blit(rotated, r)
            print(a)
            angle += angle_between
        print()

        pg.draw.line(self.screen, RED, (start_x, start_y), (start_x + hand_distance, start_y))
        pg.draw.line(self.screen, RED, (x, start_y - 10), (x + space, start_y - 10))

        # pg.draw.circle(self.screen, BLACK, (x, y), radius, 1)
        # pi = 3.14
        # start = pi
        # end = pi - start
        # rect = (0, 0, self.width / 2, self.height / 4)
        # pg.draw.rect(self.screen, RED, rect)
        # pg.draw.arc(self.screen, BLACK, rect, start, end, 1)