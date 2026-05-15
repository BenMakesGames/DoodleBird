using BenMakesGames.MonoGame.Palettes;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;
using PetDoodle.Biomes;
using PetDoodle.Data;
using PetDoodle.Encounters;
using PetDoodle.Persistence;
using PetDoodle.UI;

namespace PetDoodle.GameStates;

public sealed record AdventuringConfig(GameData GameData);

public sealed class Adventuring: GameState<AdventuringConfig>
{
    private const int BirdLeftX = 24;
    private const int EncounterTextGap = 4;

    private GraphicsManager Graphics { get; }
    private GameStateManager GSM { get; }
    private MouseManager Mouse { get; }
    private SaveService SaveService { get; }

    private GameData GameData { get; }
    private ButtonList Buttons { get; }

    public Adventuring(
        AdventuringConfig config,
        GraphicsManager graphics, GameStateManager gsm, MouseManager mouse,
        SaveService saveService
    )
    {
        Graphics = graphics;
        GSM = gsm;
        Mouse = mouse;
        SaveService = saveService;

        GameData = config.GameData;

        if (GameData.CurrentAdventure is null || GameData.CurrentAdventure.RemainingSteps.Count == 0)
            throw new InvalidOperationException("Adventuring state requires GameData.CurrentAdventure with at least one remaining step.");

        Buttons = new ButtonList(
            gsm, mouse,
            [LinkLabel.CreateBottomRight(graphics, "Continue", ResolveCurrentStep)]
        );
    }

    private AdventureStep CurrentStep =>
        GameData.CurrentAdventure is { } adventure
            ? adventure.RemainingSteps[0]
            : throw new InvalidOperationException("CurrentAdventure became null while in Adventuring state.");

    public override void Input(GameTime gameTime)
    {
        Buttons.Input();
    }

    public override void Update(GameTime gameTime)
    {
        Buttons.Update(this);
    }

    public override void Draw(GameTime gameTime)
    {
        var biomeInfo = CurrentStep.Biome.GetInfo();

        Graphics.Clear(biomeInfo.SkyColor);
        Graphics.DrawFilledRectangle(0, Graphics.Height - 8, Graphics.Width, 8, biomeInfo.GroundColor);

        var birdSheet = Graphics.SpriteSheets[Pictures.Bird];
        var groundY = Graphics.Height - 6;
        var birdCenterX = BirdLeftX + birdSheet.SpriteWidth / 2;
        var birdCenterY = groundY - birdSheet.SpriteHeight / 2;
        BirdSprite.Draw(Graphics, birdCenterX, birdCenterY, facingRight: true, frame: 0);

        var font = Graphics.Fonts["Font"];
        var textX = BirdLeftX + birdSheet.SpriteWidth + EncounterTextGap;
        var textY = birdCenterY - font.MaxCharacterHeight / 2;
        Graphics.DrawText("Font", textX, textY, CurrentStep.Encounter.GetInfo().DisplayName, DawnBringers16.White);

        Buttons.Draw(Graphics);

        Mouse.Draw(this);
    }

    private void ResolveCurrentStep()
    {
        if (GameData.CurrentAdventure is not { } adventure)
            throw new InvalidOperationException("CurrentAdventure became null while in Adventuring state.");

        adventure.RemainingSteps.RemoveAt(0);
        SaveService.Save(GameData);

        if (adventure.RemainingSteps.Count == 0)
            EndAdventure();
    }

    private void EndAdventure()
    {
        GameData.CurrentAdventure = null;
        SaveService.Save(GameData);
        GSM.ChangeState<Playing, PlayingConfig>(new(GameData));
    }
}
