using DoodleBird.Data;
using Microsoft.Xna.Framework;

namespace DoodleBird.Biomes;

public sealed record BiomeInfo(string DisplayName, Color SkyColor, Color GroundColor, Encounter[] PossibleEncounters, BirdFrame BirdFrame);
