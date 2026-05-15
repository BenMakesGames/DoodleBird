using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace DoodleBird;

public sealed class WanderingBirdView
{
    private const float BirdSpeed = 45f;
    private const float FixedTickStep = BirdSpeed / 60f;
    private const float HopWavelength = 6f;
    private const float HopAmplitude = 2f;

    public float X { get; private set; }
    public float? TargetX { get; private set; }

    private float HopPhase;
    private float? LastX;
    private bool FacingRight = true;

    public WanderingBirdView(float initialX, float? initialTargetX)
    {
        X = initialX;
        TargetX = initialTargetX;
    }

    public bool IsIdle => TargetX is null;

    public void FixedUpdate(GameTime gameTime)
    {
        if (TargetX is { } target)
        {
            if (MathF.Abs(target - X) <= FixedTickStep)
            {
                X = target;
                TargetX = null;
            }
            else
            {
                X += target > X ? FixedTickStep : -FixedTickStep;
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        if (LastX is null)
        {
            LastX = X;
            return;
        }

        if (TargetX is { } target)
        {
            HopPhase = (HopPhase + MathF.Abs(X - LastX.Value)) % HopWavelength;
            FacingRight = target > X;
        }
        else
        {
            HopPhase = 0f;
        }

        LastX = X;
    }

    public void Draw(GraphicsManager graphics)
    {
        var sheet = graphics.SpriteSheets[Pictures.Bird];

        var yOffset = -MathF.Sin((HopPhase / HopWavelength) * MathF.PI) * HopAmplitude;

        var groundY = graphics.Height - 6;
        var topLeftX = (int)MathF.Round(X);
        var topLeftY = (int)MathF.Round(groundY - sheet.SpriteHeight + yOffset);
        var centerX = topLeftX + sheet.SpriteWidth / 2;
        var centerY = topLeftY + sheet.SpriteHeight / 2;

        BirdSprite.Draw(graphics, centerX, centerY, FacingRight);
    }
}
