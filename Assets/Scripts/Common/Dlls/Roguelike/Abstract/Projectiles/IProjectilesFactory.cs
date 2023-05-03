using Roguelike.Spells;
using Roguelike.Tiles.Looting;

namespace Roguelike.Abstract.Projectiles
{
  public interface IProjectilesFactory
  {
    IProjectile CreateProjectile(Dungeons.Core.Vector2D pos, SpellKind sk);
    IProjectile CreateProjectile(Dungeons.Core.Vector2D pos, FightItemKind fik);
  }
}
