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

        // draw map
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                var tile = tiles[i, j];
                if (tile is null) continue;
                DrawTile(tile, i, j);
            }
        }
        // draw player's hands
        int x = 68;
        int y = 1;
        for (int i = 0; i < match.Players.Count; i++) {
            var player = match.Players[i];
            var nameS = " " + player.ShortStr;
            if (i == match.CurPlayerI)
                nameS = ">" + player.ShortStr + "<";
            NCurses.MoveAddString(y, x, nameS + " (" + player.Energy.ToString() + ")");
            NCurses.MoveAddString(y + 2, x, "Hand:");

            for (int ii = 0; ii < player.Hand.Cards.Count; ii++) {
                var card = player.Hand.Cards[ii];
                NCurses.MoveAddString(y + ii + 3, x + 1, card.Original.Name + " [" + card.MID + "]");
            }

            x += 40;
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
        NCurses.MoveAddString(y + 5, x + 3, "  .  ");
        NCurses.MoveAddString(y + 5, x + 2, i.ToString());
        var js = j.ToString();
        NCurses.MoveAddString(y + 5, x + 9 - js.Length, js);

        if (tile.Owner is object) {
            NCurses.MoveAddString(y + 3, x + 3, tile.Owner.Name);
        }

        if (tile.Entity is object) {
            var en = tile.Entity;
            if (en.Original.Power > 0) {
                NCurses.MoveAddString(y + 2, x + 2, en.Power.ToString());
            }
            if (en.Original.Life > 0) {
                var ls = en.Life.ToString();
                NCurses.MoveAddString(y + 2, x + 9 - ls.Length, ls);
            }
            if (en.IsUnit) {
                var ms = en.Movement.ToString();
                NCurses.MoveAddString(y + 1, x + 9 - ms.Length, ms);
            }

        }

    }
}