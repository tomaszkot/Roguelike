using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Abstract.Projectiles
{
  public interface IProjectile : Dungeons.Tiles.Abstract.IProjectile
  {
    LivingEntity Caller { get; set; }
  }
}
