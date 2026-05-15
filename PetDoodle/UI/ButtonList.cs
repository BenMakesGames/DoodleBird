using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace PetDoodle.UI;

public class ButtonList
{
    public IList<IButton> Buttons { get; }
    public IButton? Hovered { get; private set; }
    public IButton Active { get; private set; }

    private GameStateManager GSM { get; }
    private MouseManager Cursor { get; }

    public ButtonList(
        GameStateManager gsm, MouseManager cursor,
        IList<IButton> buttons
    )
    {
        if (buttons.Count == 0)
            throw new ArgumentException("ButtonList requires at least one button.", nameof(buttons));

        Buttons = buttons;
        Active = buttons[0];
        Cursor = cursor;
        GSM = gsm;
        Input(true);
    }

    public void Input(bool forceUpdate = false)
    {
        if (!GSM.IsActive)
            return;

        if(Cursor.Moved || forceUpdate)
            Hovered = Buttons.FirstOrDefault(b => b.Contains(Cursor.X, Cursor.Y));

        if (Hovered is not null)
            Active = Hovered;
    }

    private const int OffAxisWeight = 2;

    public void Update(AbstractGameState owningState)
    {
        if (GSM.CurrentState != owningState)
        {
            Hovered = null;
            Active = Buttons[0];
        }
        else if (GSM.IsActive)
        {
            var cursorWasCaptured = Buttons.Any(b => b.IsCapturingCursor);

            foreach (var button in Buttons)
                button.Update(Cursor);

            if(Cursor.LeftClicked && Cursor.IsInWindow() && Hovered is not null && !cursorWasCaptured)
                Hovered.Action();
        }

    }

    public void Draw(GraphicsManager graphics)
    {
        foreach(var button in Buttons)
            button.Draw(graphics, button == Active);
    }
}
