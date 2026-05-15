namespace DoodleBird.Data;

public class GameData
{
    public required Bird Bird { get; init; }
    public Adventure? CurrentAdventure { get; set; }
}
