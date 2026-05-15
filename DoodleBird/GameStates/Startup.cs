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

    public Startup(GraphicsManager graphics, GameStateManager gsm, MouseManager mouse, SaveService saveService)
    {
        Graphics = graphics;
        GSM = gsm;
        Mouse = mouse;
        SaveService = saveService;

        Mouse.UseCustomCursor(Pictures.Cursor, (3, 1));
    }

    // note: you do NOT need to call the `base.` for lifecycle methods. so save some CPU cycles,
    // and don't call them :P

    public override void Update(GameTime gameTime)
    {
        if (Graphics.FullyLoaded)
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

        Graphics.DrawWavyText("Font", gameTime, "Loading...", DawnBringers16.White);

        Mouse.Draw(this);
    }
}
