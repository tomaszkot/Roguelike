using Dungeons.Tiles;
using Roguelike.Tiles.Abstract;

namespace Roguelike.Abstract.Projectiles
{
  public interface IProjectile
  {
    IDestroyable Target { get; set; }
  }
}
