using DoodleBird.Data;

namespace DoodleBird.Encounters;

public abstract record Outcome(string Text);

public sealed record FlavorOutcome(string Text) : Outcome(Text);

public sealed record SubstituteOutcome(string Text, Encounter NewEncounter) : Outcome(Text);

public sealed record EndAdventureOutcome(string Text) : Outcome(Text);

public sealed record ReplaceStepsOutcome : Outcome
{
    public IReadOnlyList<Biome> Biomes { get; }

    public ReplaceStepsOutcome(string Text, IReadOnlyList<Biome> Biomes) : base(Text)
    {
        if (Biomes.Count == 0)
            throw new ArgumentException("ReplaceStepsOutcome requires at least one biome.", nameof(Biomes));
        this.Biomes = Biomes;
    }
}
