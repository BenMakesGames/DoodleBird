using BenMakesGames.MonoGame.Palettes;
using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Services;

namespace DoodleBird.UI;

public sealed record OptionButton(
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
}
