using BenMakesGames.RandomHelpers;
using DoodleBird.Biomes;
using DoodleBird.Data;

namespace DoodleBird.Adventures;

public static class AdventureGenerator
{
    private static readonly Biome[][] Templates =
    [
        [Biome.Grasslands, Biome.River, Biome.Jungle, Biome.Cave],
        [Biome.Grasslands, Biome.Mountain, Biome.MountainPeak],
        [Biome.River, Biome.River, Biome.Waterfall, Biome.Jungle],
        [Biome.Jungle, Biome.Cave, Biome.Cave],
        [Biome.Grasslands, Biome.Beach, Biome.Lagoon],
        [Biome.River, Biome.River, Biome.Lagoon],
    ];

    public static Adventure? TryRoll()
    {
        var runnable = Templates
            .Where(t => t.All(b => b.GetInfo().PossibleEncounters.Length > 0))
            .ToArray();

        if (runnable.Length == 0)
            return null;

        var template = Random.Shared.Next(runnable);

        var steps = new List<AdventureStep>(template.Length);
        foreach (var biome in template)
        {
            var encounter = Random.Shared.Next(biome.GetInfo().PossibleEncounters);
            steps.Add(new AdventureStep(biome, encounter));
        }

        return new Adventure { RemainingSteps = steps };
    }
}
