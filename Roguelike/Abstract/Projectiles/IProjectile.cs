using Dungeons.Tiles;

namespace Roguelike.Abstract.Projectiles
{
  public interface IProjectile
  {
    Tile Target { get; set; }
  }
}
