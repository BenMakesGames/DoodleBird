using BenMakesGames.MonoGame.Palettes;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace DoodleBird.GameStates;

public sealed record DialogConfig(string Text, Action OnComplete);

// sealed classes execute faster than non-sealed, so always seal your game states!
public sealed class Dialog : GameState<DialogConfig>
{
    private const int MsPerChar = 60;
    private const int MinDurationMs = 1500;

    private GraphicsManager Graphics { get; }
    private MouseManager Mouse { get; }

    private string Text { get; }
    private int TextWidth { get; }
    private int TextHeight { get; }
    private Action OnComplete { get; }
    private double TTL { get; set; }
    private bool Fired { get; set; }

    public Dialog(DialogConfig config, GraphicsManager graphics, MouseManager mouse)
    {
        Graphics = graphics;
        Mouse = mouse;

        Text = config.Text;
        OnComplete = config.OnComplete;

        var lines = Text.Split('\n');
        var longestLine = lines.Max(line => line.Length);
        TextWidth = longestLine * 6;
        TextHeight = lines.Length * 9;

        TTL = Math.Max(MinDurationMs, Text.Length * MsPerChar) / 1000.0;
    }

    public override void Input(GameTime gameTime)
    {
    }

    public override void Update(GameTime gameTime)
    {
        if (Fired)
            return;

        TTL -= gameTime.ElapsedGameTime.TotalSeconds;

        if (TTL <= 0)
        {
            Fired = true;
            OnComplete();
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Graphics.DrawPicture(Pictures.DialogBorder, 0, 0);

        Graphics.DrawText(
            "Font",
            (Graphics.Width - TextWidth) / 2,
            (Graphics.Height - TextHeight) / 2,
            Text,
            DawnBringers16.LightGray
        );

        Mouse.Draw(this);
    }
}
