using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework.Graphics;

namespace DoodleBird;

public static class BirdSprite
{
    public static void Draw(GraphicsManager graphics, int centerX, int centerY, bool facingRight, int frame = 0)
    {
        var sheet = graphics.SpriteSheets[Pictures.Bird];
        var topLeftX = centerX - sheet.SpriteWidth / 2;
        var topLeftY = centerY - sheet.SpriteHeight / 2;

        graphics.DrawSpriteFlipped(
            Pictures.Bird,
            topLeftX, topLeftY,
            frame,
            facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally
        );
    }
}
