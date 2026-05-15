using BenMakesGames.MonoGame.Palettes;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;
using PetDoodle.Adventures;
using PetDoodle.Data;
using PetDoodle.Persistence;

namespace PetDoodle.GameStates;

public sealed record PlayingConfig(GameData GameData);

// sealed classes execute faster than non-sealed, so always seal your game states!
public sealed class Playing: GameState<PlayingConfig>
{
    private const float InitialBirdX = 20f;
    private const float InitialBirdTargetX = 100f;
    private const float IdleSecondsToAdventure = 3f;

    private GraphicsManager Graphics { get; }
    private GameStateManager GSM { get; }
    private MouseManager Mouse { get; }
    private SaveService SaveService { get; }

    private GameData GameData { get; }
    private WanderingBirdView BirdView { get; }

    private float IdleSeconds;

    public Playing(
        PlayingConfig config,
        GraphicsManager graphics, GameStateManager gsm, MouseManager mouse,
        SaveService saveService
    )
    {
        Graphics = graphics;
        GSM = gsm;
        Mouse = mouse;
        SaveService = saveService;

        GameData = config.GameData;
        BirdView = new WanderingBirdView(InitialBirdX, InitialBirdTargetX);
    }

    public override void Enter()
    {
        if (GameData.CurrentAdventure is not null)
            GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData));
    }

    public override void Input(GameTime gameTime)
    {
    }

    public override void FixedUpdate(GameTime gameTime)
    {
        BirdView.FixedUpdate(gameTime);

        if (GameData.CurrentAdventure is not null)
            return;

        if (BirdView.IsIdle)
        {
            IdleSeconds += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (IdleSeconds >= IdleSecondsToAdventure)
                TryBeginAdventure();
        }
        else
        {
            IdleSeconds = 0f;
        }
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

    private void TryBeginAdventure()
    {
        var adventure = AdventureGenerator.TryRoll();
        if (adventure is null)
        {
            IdleSeconds = 0f;
            return;
        }

        GameData.CurrentAdventure = adventure;
        SaveService.Save(GameData);
        GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData));
    }
}
