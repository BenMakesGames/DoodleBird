using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PetDoodle.Data;

namespace PetDoodle;

public sealed class BirdRenderer
{
    private const float HopWavelength = 6f;
    private const float HopAmplitude = 2f;

    private float HopPhase;
    private float? LastX;
    private bool FacingRight = true;

    public void Update(Bird bird, GameTime gameTime)
    {
        if (LastX is null)
        {
            LastX = bird.X;
            return;
        }

        if (bird.TargetX is { } target)
        {
            HopPhase = (HopPhase + MathF.Abs(bird.X - LastX.Value)) % HopWavelength;
            if (target != bird.X)
                FacingRight = target > bird.X;
        }
        else
        {
            HopPhase = 0f;
        }

        LastX = bird.X;
    }

    public void Draw(Bird bird, GraphicsManager graphics)
    {
        var texture = graphics.SpriteSheets[Pictures.Bird];

        var yOffset = -MathF.Sin((HopPhase / HopWavelength) * MathF.PI) * HopAmplitude;

        var groundY = graphics.Height - 6;
        var centerX = (int)MathF.Round(bird.X);
        var centerY = (int)MathF.Round(groundY - texture.SpriteHeight + yOffset);

        graphics.DrawSpriteFlipped(
            Pictures.Bird,
            centerX, centerY,
            0,
            FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally
        );
    }
}
