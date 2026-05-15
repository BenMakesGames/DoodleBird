using BenMakesGames.MonoGame.Palettes;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;
using PetDoodle.Data;

namespace PetDoodle.GameStates;

public sealed record PlayingConfig(GameData GameData);

// sealed classes execute faster than non-sealed, so always seal your game states!
public sealed class Playing: GameState<PlayingConfig>
{
    private const float InitialBirdX = 20f;
    private const float InitialBirdTargetX = 100f;

    private GraphicsManager Graphics { get; }
    private GameStateManager GSM { get; }
    private MouseManager Mouse { get; }

    private GameData GameData { get; }
    private WanderingBirdView BirdView { get; }

    public Playing(
        PlayingConfig config,
        GraphicsManager graphics, GameStateManager gsm, MouseManager mouse
    )
    {
        Graphics = graphics;
        GSM = gsm;
        Mouse = mouse;

        GameData = config.GameData;
        BirdView = new WanderingBirdView(InitialBirdX, InitialBirdTargetX);
    }

    // overriding lifecycle methods is optional; feel free to delete any overrides you're not using.
    // note: you do NOT need to call the `base.` for lifecycle methods. so save some CPU cycles,
    // and don't call them :P

    public override void Input(GameTime gameTime)
    {
        // TODO: get input from keyboard, mouse, or gamepad (refer to PlayPlayMini documentation for more info)
    }

    public override void FixedUpdate(GameTime gameTime)
    {
        BirdView.FixedUpdate(gameTime);
    }

    public override void Update(GameTime gameTime)
    {
        BirdView.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        Graphics.Clear(DawnBringers16.LightBlue);

        Graphics.DrawFilledRectangle(0, Graphics.Height - 8, Graphics.Width, 8, DawnBringers16.DarkGreen);
        Graphics.DrawPicture(Pictures.TopGrass, 0, Graphics.Height - 10);

        BirdView.Draw(Graphics);

        Mouse.Draw(this);
    }
}
