using System.Collections.Frozen;
using BenMakesGames.MonoGame.Palettes;
using PetDoodle.Data;

namespace PetDoodle.Biomes;

public static class BiomeExtensions
{
    private static readonly FrozenDictionary<Biome, BiomeInfo> Info;

    static BiomeExtensions()
    {
        Info = new Dictionary<Biome, BiomeInfo>
        {
            [Biome.Grasslands]   = new("Grasslands",    DawnBringers16.LightBlue, DawnBringers16.DarkGreen, [Encounter.HollowLog, Encounter.Snake, Encounter.LoneTree]),
            [Biome.River]        = new("River",         DawnBringers16.LightBlue, DawnBringers16.Blue,      [Encounter.MusclyTrout, Encounter.Rapids]),
            [Biome.Jungle]       = new("Jungle",        DawnBringers16.Green,     DawnBringers16.DarkGreen, []),
            [Biome.Cave]         = new("Cave",          DawnBringers16.Black,     DawnBringers16.Brown,     []),
            [Biome.Mountain]     = new("Mountain",      DawnBringers16.LightGray, DawnBringers16.DarkGray,  []),
            [Biome.MountainPeak] = new("Mountain Peak", DawnBringers16.White,     DawnBringers16.LightGray, []),
            [Biome.Waterfall]    = new("Waterfall",     DawnBringers16.LightBlue, DawnBringers16.Blue,      []),
            [Biome.Beach]        = new("Beach",         DawnBringers16.LightBlue, DawnBringers16.Yellow,    []),
            [Biome.Lagoon]       = new("Lagoon",        DawnBringers16.LightBlue, DawnBringers16.DarkBlue,  []),
        }.ToFrozenDictionary();

        if (Info.Count != Enum.GetValues<Biome>().Length)
            throw new InvalidOperationException(
                $"BiomeExtensions.Info has {Info.Count} entries but Biome enum has {Enum.GetValues<Biome>().Length} values."
            );
    }

    public static BiomeInfo GetInfo(this Biome biome) => Info[biome];
}
