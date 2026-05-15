using Microsoft.Xna.Framework;
using PetDoodle.Data;

namespace PetDoodle.Biomes;

public sealed record BiomeInfo(string DisplayName, Color SkyColor, Color GroundColor, Encounter[] PossibleEncounters);
