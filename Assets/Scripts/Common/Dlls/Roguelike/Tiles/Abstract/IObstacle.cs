using Algorithms;

namespace Roguelike.Tiles.Abstract
{
  public interface IObstacle : Dungeons.Tiles.Abstract.IObstacle
  {
    bool CanBeHitBySpell();
  }
}
