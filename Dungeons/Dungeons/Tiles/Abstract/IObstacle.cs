using Dungeons.Tiles.Abstract;
using System.Drawing;

namespace Dungeons.Tiles
{
  public interface IObstacle
  {
    bool OnHitBy(ISpell md);
    Point Position { get; }
  }
}
