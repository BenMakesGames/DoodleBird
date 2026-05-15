using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Attributes.DI;

namespace DoodleBird.Persistence;

[AutoRegister]
public sealed class TipService: IServiceLoadContent
{
    private static readonly string TipFilePath = Path.Combine(DirectoryHelpers.SaveDirectory, "NextTip");

    private int TipIndex { get; set; }

    private static readonly string[] Tips =
    [
        "The Doodlebird\nis flightless.",
        "Do not engage\na Griffin.",
        "The Doodlebird\nis patient.",
        "Anything anyone\never thought of\nis in The Umbra.",
    ];

    public string CurrentTip => Tips[TipIndex];

    public void LoadContent(GameStateManager gsm)
    {
        TipIndex = GetTipIndex();
    }

    private static void CreateNewTipFile()
    {
        try
        {
            File.WriteAllBytes(TipFilePath, new byte[] { 1 });
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private int GetTipIndex()
    {
        if (!File.Exists(TipFilePath))
        {
            CreateNewTipFile();

            return 0;
        }

        try
        {
            using var file = File.Open(TipFilePath, FileMode.Open);

            if (file.Length != 1)
            {
                file.Close();
                CreateNewTipFile();
                return 0;
            }

            var tipIndex = file.ReadByte() % Tips.Length;

            file.Position = 0;
            file.WriteByte((byte)((tipIndex + 1) % Tips.Length));
            file.Close();

            return tipIndex;
        }
        catch (Exception)
        {
            CreateNewTipFile();
            return 0;
        }
    }

    public void UnloadContent()
    {
    }

    public bool FullyLoaded => true;
}
