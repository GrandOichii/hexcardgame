import curses

def draw_list(window: curses.window, y: int, x:int, options: list[str], max_display_amount: int, cursor: int, page_n: int, focus_selected: bool=True):
    for i in range(min(max_display_amount, len(options))):
        if i == cursor and focus_selected:
            window.addstr(i + y, x, options[i + page_n], curses.A_REVERSE)
        else:
            window.addstr(i + y, x, options[i + page_n])

class ListTemplate:
    def __init__(self, window: curses.window, options: list[str], max_display_amount: int, ):
        self.window = window
        self.options = options
        self.max_display_amount = max_display_amount

        self._reset()

    def _reset(self):
        self.cursor = 0
        self.choice = 0
        self.page_n = 0

    # TODO not tested
    def remove_current(self):
        del self.options[self.choice]
        self._reset()

    def selected(self):
        if self.choice >= len(self.options):
            return None
        return self.options[self.choice]

    def draw(self, y: int, x: int, focus_selected: bool=True):
        draw_list(self.window, y, x + 1, self.options, self.max_display_amount, self.cursor, self.page_n, focus_selected)
        if len(self.options) < self.max_display_amount: return

        if self.page_n != 0:
            self.window.addch(y, x, curses.ACS_UARROW)
        if self.page_n != len(self.options) - self.max_display_amount:
            self.window.addch(y + self.max_display_amount - 1, x, curses.ACS_DARROW)

    def scroll_up(self):
        self.choice -= 1
        self.cursor -= 1
        if self.cursor < 0:
            if len(self.options) > self.max_display_amount:
                if self.page_n == 0:
                    self.cursor = self.max_display_amount - 1
                    self.choice = len(self.options) - 1
                    self.page_n = len(self.options) - self.max_display_amount
                else:
                    self.page_n -= 1
                    self.cursor += 1
            else:
                self.cursor = len(self.options) - 1
                self.choice = self.cursor

    def scroll_down(self):
        self.choice += 1
        self.cursor += 1
        if len(self.options) > self.max_display_amount:
            if self.cursor >= self.max_display_amount:
                self.cursor -= 1
                self.page_n += 1
                if self.choice == len(self.options):
                    self.choice = 0
                    self.cursor = 0
                    self.page_n = 0
        else:
            if self.cursor >= len(self.options):
                self.cursor = 0
                self.choice = 0


