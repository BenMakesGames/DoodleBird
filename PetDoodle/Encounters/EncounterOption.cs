namespace PetDoodle.Encounters;

public sealed record EncounterOption
{
    public required string Label { get; init; }
    public required OptionKind Kind { get; init; }

    private readonly Outcome[] _outcomes = null!;
    public required Outcome[] Outcomes
    {
        get => _outcomes;
        init
        {
            if (value is null || value.Length == 0)
                throw new ArgumentException("EncounterOption.Outcomes must be non-null and non-empty.", nameof(value));

            _outcomes = value;
        }
    }
}
