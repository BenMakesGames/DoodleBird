using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PetDoodle.Data;
using Serilog;

namespace PetDoodle.Persistence;

public sealed class SaveService
{
    private static readonly string SaveFilePath = Path.Combine(DirectoryHelpers.SaveDirectory, "save.json");
    private static readonly string TempFilePath = SaveFilePath + ".tmp";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public GameData? Load()
    {
        if (!File.Exists(SaveFilePath))
            return null;

        try
        {
            var json = File.ReadAllText(SaveFilePath);
            return JsonSerializer.Deserialize<GameData>(json, Options);
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialise save file at {Path}; starting fresh.", SaveFilePath);
            return null;
        }
    }

    public void Save(GameData data)
    {
        var json = JsonSerializer.Serialize(data, Options);
        File.WriteAllText(TempFilePath, json);
        File.Move(TempFilePath, SaveFilePath, overwrite: true);
    }
}
