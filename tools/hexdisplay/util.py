import curses
from curses.textpad import rectangle

def box(win: curses.window, y: int, x: int, height: int, width: int, continue_up: bool=False, continue_down:bool=False, attr: int=0):
    height -= 1
    width -= 1
    win.attron(attr)
    rectangle(win, y, x, y+height, x+width)
    win.attroff(attr)

    if continue_down:
        win.addch(y+height, x, curses.ACS_LTEE, attr)
        win.addch(y+height, x+width, curses.ACS_RTEE, attr)
    if continue_up:
        win.addch(y, x, curses.ACS_LTEE, attr)
        win.addch(y, x+width, curses.ACS_RTEE, attr)