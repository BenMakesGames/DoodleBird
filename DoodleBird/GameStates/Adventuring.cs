using System.Diagnostics;
using BenMakesGames.MonoGame.Palettes;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Services;
using BenMakesGames.RandomHelpers;
using DoodleBird.Biomes;
using DoodleBird.Data;
using DoodleBird.Encounters;
using DoodleBird.Persistence;
using DoodleBird.UI;
using Microsoft.Xna.Framework;

namespace DoodleBird.GameStates;

public sealed record AdventuringConfig(GameData GameData);

public sealed class Adventuring: GameState<AdventuringConfig>
{
    private const int BirdLeftX = 24;
    private const int EncounterTextTopY = 2;
    private const float OptionTimerSeconds = 5f;
    private const int OptionRowY = 22;
    private const int OptionRowHeight = 10;
    private const int OptionRowSideMargin = 2;
    private const int TimerBarHeight = 3;

    private GraphicsManager Graphics { get; }
    private GameStateManager GSM { get; }
    private MouseManager Mouse { get; }
    private SaveService SaveService { get; }

    private GameData GameData { get; }

    private ButtonList Buttons = null!;
    private float RemainingSeconds;

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

        EnterCurrentStep();
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

    public override void FixedUpdate(GameTime gameTime)
    {
        RemainingSeconds -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (RemainingSeconds <= 0f)
            Buttons.Active.Action();
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
        BirdSprite.Draw(Graphics, birdCenterX, birdCenterY, facingRight: true, frame: (int)biomeInfo.BirdFrame);

        var font = Graphics.Fonts["Font"];
        var encounterName = CurrentStep.Encounter.GetInfo().DisplayName;
        var textX = (Graphics.Width - font.ComputeWidth(encounterName)) / 2;
        Graphics.DrawText("Font", textX, EncounterTextTopY, encounterName, DawnBringers16.White);

        Buttons.Draw(Graphics);

        var fraction = MathF.Max(RemainingSeconds, 0f) / OptionTimerSeconds;
        var barWidth = (int)(Graphics.Width * fraction);
        Graphics.DrawFilledRectangle(0, Graphics.Height - TimerBarHeight, barWidth, TimerBarHeight, DawnBringers16.Red);

        Mouse.Draw(this);
    }

    private void EnterCurrentStep()
    {
        var options = CurrentStep.Encounter.GetInfo().Options;
        if (options.Length == 0)
            throw new InvalidOperationException($"Encounter {CurrentStep.Encounter} has no options.");

        var slotCount = options.Length;
        var innerWidth = Graphics.Width - OptionRowSideMargin * 2;

        var buttons = new IButton[slotCount];
        for (var i = 0; i < slotCount; i++)
        {
            var option = options[i];
            var slotStart = OptionRowSideMargin + (innerWidth * i) / slotCount;
            var slotEnd = OptionRowSideMargin + (innerWidth * (i + 1)) / slotCount;
            buttons[i] = new OptionButton(
                slotStart, OptionRowY, slotEnd - slotStart, OptionRowHeight,
                option.Label, () => FireOption(option)
            );
        }

        var defaultIndex = Random.Shared.Next(slotCount);
        Buttons = new ButtonList(GSM, Mouse, buttons, buttons[defaultIndex]);

        RemainingSeconds = OptionTimerSeconds;
    }

    private void FireOption(EncounterOption option)
    {
        var outcome = Random.Shared.Next(option.Outcomes);
        GSM.ChangeState<Dialog, DialogConfig>(new(outcome.Text, () => ApplyOutcome(outcome)));
    }

    private void ApplyOutcome(Outcome outcome)
    {
        if (GameData.CurrentAdventure is not { } adventure)
            throw new InvalidOperationException("CurrentAdventure became null while applying outcome.");

        switch (outcome)
        {
            case FlavorOutcome:
                adventure.RemainingSteps.RemoveAt(0);
                if (adventure.RemainingSteps.Count == 0)
                {
                    GameData.CurrentAdventure = null;
                    SaveService.Save(GameData);
                    GSM.ChangeState<Playing, PlayingConfig>(new(GameData));
                }
                else
                {
                    SaveService.Save(GameData);
                    GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData));
                }
                break;
            case SubstituteOutcome s:
                adventure.RemainingSteps[0] = new AdventureStep(adventure.RemainingSteps[0].Biome, s.NewEncounter);
                SaveService.Save(GameData);
                GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData));
                break;
            case EndAdventureOutcome:
                GameData.CurrentAdventure = null;
                SaveService.Save(GameData);
                GSM.ChangeState<Playing, PlayingConfig>(new(GameData));
                break;
            case ReplaceStepsOutcome r:
                ReplaceRemainingSteps(adventure, r.Biomes);
                break;
            default:
                throw new UnreachableException($"Unhandled Outcome subtype: {outcome.GetType().Name}");
        }
    }

    private void ReplaceRemainingSteps(Adventure adventure, IReadOnlyList<Biome> biomes)
    {
        var newSteps = new List<AdventureStep>(biomes.Count);
        foreach (var biome in biomes)
        {
            var pool = biome.GetInfo().PossibleEncounters;
            if (pool.Length == 0)
                throw new InvalidOperationException(
                    $"Cannot replace remaining steps with biome '{biome}': PossibleEncounters is empty."
                );
            newSteps.Add(new AdventureStep(biome, Random.Shared.Next(pool)));
        }

        adventure.RemainingSteps.Clear();
        adventure.RemainingSteps.AddRange(newSteps);
        SaveService.Save(GameData);
        GSM.ChangeState<Adventuring, AdventuringConfig>(new(GameData));
    }
}
