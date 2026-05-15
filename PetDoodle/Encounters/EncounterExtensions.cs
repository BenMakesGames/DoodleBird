using System.Collections.Frozen;
using PetDoodle.Data;

namespace PetDoodle.Encounters;

public static class EncounterExtensions
{
    private static readonly FrozenDictionary<Encounter, EncounterInfo> Info;

    static EncounterExtensions()
    {
        Info = new Dictionary<Encounter, EncounterInfo>().ToFrozenDictionary();

        foreach (var (encounter, info) in Info)
        {
            foreach (var option in info.Options)
            {
                if (option.Outcomes is null || option.Outcomes.Length == 0)
                    throw new InvalidOperationException(
                        $"Encounter {encounter} option '{option.Label}' has no outcomes."
                    );
            }
        }
    }

    public static EncounterInfo GetInfo(this Encounter encounter) => Info[encounter];
}
