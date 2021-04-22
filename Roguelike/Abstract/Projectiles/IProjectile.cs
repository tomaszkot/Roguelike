using Dungeons.Tiles;
using Roguelike.Tiles.Abstract;

namespace Roguelike.Abstract.Projectiles
{
  public interface IProjectile
  {
    Dungeons.Tiles.IObstacle Target { get; set; }
  }
}
