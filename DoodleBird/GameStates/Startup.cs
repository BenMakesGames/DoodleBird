using BenMakesGames.MonoGame.Palettes;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Services;
using DoodleBird.Data;
using DoodleBird.Persistence;
using Microsoft.Xna.Framework;

namespace DoodleBird.GameStates;

// inheriting game states is a path that leads to madness, so always seal your game states!
public sealed class Startup: GameState
{
    private GraphicsManager Graphics { get; }
    private GameStateManager GSM { get; }
    private MouseManager Mouse { get; }
    private SaveService SaveService { get; }

    private int TipWidth { get; }
    private int TipHeight { get; }
    private string Tip { get; }
    private double TTL { get; set; } = 3;

    public Startup(
        GraphicsManager graphics, GameStateManager gsm, MouseManager mouse, SaveService saveService,
        TipService tipService
    )
    {
        Graphics = graphics;
        GSM = gsm;
        Mouse = mouse;
        SaveService = saveService;

        Tip = tipService.CurrentTip;

        var lines = Tip.Split('\n');
        var longestLine = lines.Max(line => line.Length);
        TipWidth = longestLine * 6;
        TipHeight = lines.Length * 9;

        Mouse.UseCustomCursor(Pictures.Cursor, (3, 1));
    }

    public override void Update(GameTime gameTime)
    {
        TTL -= gameTime.ElapsedGameTime.TotalSeconds;

        if (TTL <= 0 && Graphics.FullyLoaded)
        {
            var gameData = SaveService.Load() ?? new GameData()
            {
                Bird = new Bird()
                {
                    Name = "Alain",
                }
            };

            GSM.Window.Title = gameData.Bird.Name;

            GSM.ChangeState<Playing, PlayingConfig>(new(gameData));
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Graphics.Clear(DawnBringers16.Black);

        Graphics.DrawText("Font", (Graphics.Width - TipWidth) / 2, (Graphics.Height - TipHeight) / 2, Tip, DawnBringers16.LightGray);

        Mouse.Draw(this);
    }
}
