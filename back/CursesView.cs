using core.match;
using core.tiles;
using Mindmagma.Curses;

class CursesMatchView : MatchView
{
    static string[] TILE_SPRITE = new string[] {
        "   -----   ",
        " /       \\ ",
        "/         \\",
        "           ",
        "\\         /",
        " \\       / ",
        "   -----   "  
    };

    public IntPtr Screen { get; }
    public CursesMatchView() {
        Screen = NCurses.InitScreen();
        // NCurses.NoDelay(Screen, true);
        NCurses.NoEcho();
        NCurses.SetCursor(0);
    }
    
    public override void Start()
    {
    }

    public override void Update(Match match)
    {
        var tiles = match.Map.Tiles;
        var height = match.Map.Height;
        var width = match.Map.Width;

        NCurses.Clear();
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                var tile = tiles[i, j];
                if (tile is null) continue;
                DrawTile(tile, i, j);
            }
        }
        NCurses.Refresh();

        // var key = NCurses.GetChar();
        // if (key == 'q') match.Winner = match.CurrentPlayer;
    }

    public override void End()
    {
        NCurses.Echo();
        NCurses.SetCursor(1);
        NCurses.EndWin();
    }

    /// <summary>
    /// Draw the tile with the specified i and j coords
    /// </summary>
    /// <param name="i">i coord</param>
    /// <param name="j">j coord</param>
    public void DrawTile(Tile tile, int i, int j) {
        int len = TILE_SPRITE.Length;
        int y = (len / 2) * i + 1;
        int b = 9;
        int x = b * 2 * j + (1 - i % 2) * b;
        for (int ii = 0; ii < len; ii++) {
            NCurses.MoveAddString(y + ii, x, TILE_SPRITE[ii]);
        }
        if (tile.Owner is null) return;

        NCurses.MoveAddString(y + 3, x + 3, tile.Owner.Name);
    }
}