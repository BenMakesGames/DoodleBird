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
    private const int EncounterTextGap = 4;
    private const float OptionTimerSeconds = 5f;
    private const int OptionRowY = 22;
    private const int OptionRowHeight = 10;
    private const int OptionRowSideMargin = 2;
    private const int TimerTopMargin = 1;
    private const int TimerRightMargin = 2;
    private const float OutcomeDisplaySeconds = 2.5f;

    private enum Phase { ChoosingOption, ShowingOutcome }

    private GraphicsManager Graphics { get; }
    private GameStateManager GSM { get; }
    private MouseManager Mouse { get; }
    private SaveService SaveService { get; }

    private GameData GameData { get; }

    private ButtonList Buttons = null!;
    private float RemainingSeconds;
    private Phase CurrentPhase = Phase.ChoosingOption;
    private Outcome? PendingOutcome;
    private float OutcomeRemainingSeconds;

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
        if (CurrentPhase != Phase.ChoosingOption)
            return;

        Buttons.Input();
    }

    public override void Update(GameTime gameTime)
    {
        if (CurrentPhase != Phase.ChoosingOption)
            return;

        Buttons.Update(this);
    }

    public override void FixedUpdate(GameTime gameTime)
    {
        switch (CurrentPhase)
        {
            case Phase.ChoosingOption:
                TickOptionTimer(gameTime);
                break;
            case Phase.ShowingOutcome:
                TickOutcome(gameTime);
                break;
        }
    }

    private void TickOptionTimer(GameTime gameTime)
    {
        RemainingSeconds -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (RemainingSeconds <= 0f)
            Buttons.Active.Action();
    }

    private void TickOutcome(GameTime gameTime)
    {
        if (PendingOutcome is null)
            return;

        OutcomeRemainingSeconds -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (OutcomeRemainingSeconds > 0f)
            return;

        var outcome = PendingOutcome;
        PendingOutcome = null;
        ApplyOutcome(outcome);
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

        switch (CurrentPhase)
        {
            case Phase.ChoosingOption:
                Buttons.Draw(Graphics);

                var timerText = $"{MathF.Max(RemainingSeconds, 0f):0.0}";
                var timerWidth = font.ComputeWidth(timerText);
                var timerX = Graphics.Width - timerWidth - TimerRightMargin;
                Graphics.DrawText("Font", timerX, TimerTopMargin, timerText, DawnBringers16.White);
                break;
            case Phase.ShowingOutcome when PendingOutcome is { } outcome:
                var outcomeWidth = font.ComputeWidth(outcome.Text);
                var outcomeX = (Graphics.Width - outcomeWidth) / 2;
                var outcomeY = OptionRowY + (OptionRowHeight - font.MaxCharacterHeight) / 2;
                Graphics.DrawText("Font", outcomeX, outcomeY, outcome.Text, DawnBringers16.White);
                break;
        }

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
        CurrentPhase = Phase.ChoosingOption;
    }

    private void FireOption(EncounterOption option)
    {
        PendingOutcome = Random.Shared.Next(option.Outcomes);
        OutcomeRemainingSeconds = OutcomeDisplaySeconds;
        CurrentPhase = Phase.ShowingOutcome;
    }

    private void ApplyOutcome(Outcome outcome)
    {
        switch (outcome)
        {
            case FlavorOutcome:
                ResolveCurrentStep();
                break;
            case SubstituteOutcome s:
                SubstituteCurrentEncounter(s.NewEncounter);
                break;
            case EndAdventureOutcome:
                EndAdventure();
                break;
            default:
                throw new UnreachableException($"Unhandled Outcome subtype: {outcome.GetType().Name}");
        }
    }

    private void SubstituteCurrentEncounter(Encounter newEncounter)
    {
        if (GameData.CurrentAdventure is not { } adventure)
            throw new InvalidOperationException("CurrentAdventure became null while in Adventuring state.");

        var currentBiome = CurrentStep.Biome;
        adventure.RemainingSteps[0] = new AdventureStep(currentBiome, newEncounter);
        SaveService.Save(GameData);
        EnterCurrentStep();
    }

    private void ResolveCurrentStep()
    {
        if (GameData.CurrentAdventure is not { } adventure)
            throw new InvalidOperationException("CurrentAdventure became null while in Adventuring state.");

        adventure.RemainingSteps.RemoveAt(0);
        SaveService.Save(GameData);

        if (adventure.RemainingSteps.Count == 0)
            EndAdventure();
        else
            EnterCurrentStep();
    }

    private void EndAdventure()
    {
        GameData.CurrentAdventure = null;
        SaveService.Save(GameData);
        GSM.ChangeState<Playing, PlayingConfig>(new(GameData));
    }
}
