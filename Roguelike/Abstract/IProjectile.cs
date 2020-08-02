using Dungeons.Tiles;

namespace Roguelike.Abstract
{
  public interface IProjectile
  {
    Tile Target { get; set; }
  }
}
