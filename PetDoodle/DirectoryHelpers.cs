namespace PetDoodle;

public static class DirectoryHelpers
{
    private static readonly string AppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static readonly string PetDoodleDirectory = $"{AppDataDirectory}{Path.DirectorySeparatorChar}PetDoodle";

    public static readonly string LogDirectory = $"{PetDoodleDirectory}{Path.DirectorySeparatorChar}Logs";
    public static readonly string SaveDirectory = $"{PetDoodleDirectory}{Path.DirectorySeparatorChar}Saves";

    public static void EnsureDirectoryExists()
    {
        Directory.CreateDirectory(PetDoodleDirectory);
        Directory.CreateDirectory(LogDirectory);
        Directory.CreateDirectory(SaveDirectory);
    }
}
