using Algorithms;

namespace Roguelike.Tiles.Abstract
{
  public interface IObstacle : Dungeons.Tiles.IObstacle
  {
    bool CanBeHitBySpell();
  }
}
