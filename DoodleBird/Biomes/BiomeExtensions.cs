using System.Collections.Frozen;
using BenMakesGames.MonoGame.Palettes;
using DoodleBird.Data;

namespace DoodleBird.Biomes;

public static class BiomeExtensions
{
    private static readonly FrozenDictionary<Biome, BiomeInfo> Info;

    static BiomeExtensions()
    {
        Info = new Dictionary<Biome, BiomeInfo>
        {
            [Biome.Grasslands]   = new("Grasslands",    DawnBringers16.LightBlue, DawnBringers16.DarkGreen, [Encounter.HollowLog, Encounter.Snake, Encounter.LoneTree]),
            [Biome.River]        = new("River",         DawnBringers16.LightBlue, DawnBringers16.Blue,      [Encounter.MusclyTrout, Encounter.Rapids]),
            [Biome.Jungle]       = new("Jungle",        DawnBringers16.Green,     DawnBringers16.DarkGreen, [Encounter.CarnivorousPlant, Encounter.Quicksand, Encounter.NanerTree, Encounter.LongAbandonedVillage]),
            [Biome.Cave]         = new("Cave",          DawnBringers16.Black,     DawnBringers16.Brown,     [Encounter.GlowingMushrooms, Encounter.GiantBat, Encounter.LargeBoulder]),
            [Biome.Mountain]     = new("Mountain",      DawnBringers16.LightGray, DawnBringers16.DarkGray,  [Encounter.LimestoneGolem, Encounter.SteepClimb, Encounter.Griffin]),
            [Biome.MountainPeak] = new("Mountain Peak", DawnBringers16.White,     DawnBringers16.LightGray, [Encounter.Thunderstorm]),
            [Biome.Waterfall]    = new("Waterfall",     DawnBringers16.LightBlue, DawnBringers16.Blue,      [Encounter.WaterfallDrop]),
            [Biome.Beach]        = new("Beach",         DawnBringers16.LightBlue, DawnBringers16.Yellow,    [Encounter.Sandcastle, Encounter.PurpleSeaweed, Encounter.AggressiveSeagull]),
            [Biome.Lagoon]       = new("Lagoon",        DawnBringers16.LightBlue, DawnBringers16.DarkBlue,  [Encounter.MusclyTrout, Encounter.Mermaid]),
        }.ToFrozenDictionary();

        if (Info.Count != Enum.GetValues<Biome>().Length)
            throw new InvalidOperationException(
                $"BiomeExtensions.Info has {Info.Count} entries but Biome enum has {Enum.GetValues<Biome>().Length} values."
            );
    }

    public static BiomeInfo GetInfo(this Biome biome) => Info[biome];
}
