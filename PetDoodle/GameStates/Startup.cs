using BenMakesGames.MonoGame.Palettes;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;
using PetDoodle.Data;

namespace PetDoodle.GameStates;

// inheriting game states is a path that leads to madness, so always seal your game states!
public sealed class Startup: GameState
{
    private GraphicsManager Graphics { get; }
    private GameStateManager GSM { get; }
    private MouseManager Mouse { get; }

    public Startup(GraphicsManager graphics, GameStateManager gsm, MouseManager mouse)
    {
        Graphics = graphics;
        GSM = gsm;
        Mouse = mouse;

        Mouse.UseCustomCursor(Pictures.Cursor, (3, 1));
    }

    // note: you do NOT need to call the `base.` for lifecycle methods. so save some CPU cycles,
    // and don't call them :P

    public override void Update(GameTime gameTime)
    {
        if (Graphics.FullyLoaded)
        {
            var gameData = new GameData()
            {
                Bird = new Bird()
                {
                    X = 20,
                    TargetX = 100,
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
