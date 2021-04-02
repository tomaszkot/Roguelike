using Roguelike.Spells;

namespace Roguelike.Abstract.Projectiles
{
  public interface IProjectilesFactory
  {
    IProjectile CreateProjectile(Dungeons.Core.Vector2D pos, SpellKind sk);
  }
}
