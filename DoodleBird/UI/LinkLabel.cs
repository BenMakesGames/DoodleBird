using BenMakesGames.MonoGame.Palettes;
using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Services;

namespace DoodleBird.UI;

public sealed record LinkLabel(
    int X,
    int Y,
    int Width,
    int Height,
    string Label,
    Action Action
): IButton
{
    public void Draw(GraphicsManager graphics, bool isActive)
    {
        var color = isActive ? DawnBringers16.LightBlue : DawnBringers16.Blue;

        var font = graphics.Fonts["Font"];
        var textWidth = font.ComputeWidth(Label);
        var textX = X + (Width - textWidth) / 2;
        var blockHeight = font.MaxCharacterHeight + 2;
        var textY = Y + (Height - blockHeight) / 2;
        graphics.DrawTextWithOutline("Font", textX, textY, Label, color, DawnBringers16.Black);
        var underlineY = textY + font.MaxCharacterHeight + 1;
        graphics.DrawFilledRectangle(textX, underlineY, textWidth, 1, color);
        graphics.DrawRectangle(textX - 1, underlineY - 1, textWidth + 2, 3, DawnBringers16.Black);
    }

    public static LinkLabel CreateBottomRight(GraphicsManager graphics, string label, Action action)
    {
        const int paddingX = 2;
        const int paddingY = 1;
        const int marginRight = 2;
        const int marginBottom = 2;

        var font = graphics.Fonts["Font"];
        var width = font.ComputeWidth(label) + paddingX * 2;
        var height = font.MaxCharacterHeight + 2 + paddingY * 2;
        var x = graphics.Width - width - marginRight;
        var y = graphics.Height - height - marginBottom;

        return new LinkLabel(x, y, width, height, label, action);
    }

    public static LinkLabel CreateCentered(GraphicsManager graphics, int y, string label, Action action)
    {
        const int paddingX = 2;
        const int paddingY = 1;

        var font = graphics.Fonts["Font"];
        var width = font.ComputeWidth(label) + paddingX * 2;
        var height = font.MaxCharacterHeight + 2 + paddingY * 2;
        var x = (graphics.Width - width) / 2;

        return new LinkLabel(x, y, width, height, label, action);
    }
}
