using DoodleBird.Data;

namespace DoodleBird.Encounters;

public abstract record Outcome(string Text);

public sealed record FlavorOutcome(string Text) : Outcome(Text);

public sealed record SubstituteOutcome(string Text, Encounter NewEncounter) : Outcome(Text);

public sealed record EndAdventureOutcome(string Text) : Outcome(Text);
