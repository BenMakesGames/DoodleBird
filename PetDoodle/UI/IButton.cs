using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Services;

namespace PetDoodle.UI;

public interface IButton: IRectangle<int>
{
    Action Action { get; }
    void Draw(GraphicsManager graphics, bool isActive);

    bool TryConsumeHorizontalInput(bool left, bool right) => false;
    void Update(MouseManager mouse) { }
    bool IsCapturingCursor => false;
}
