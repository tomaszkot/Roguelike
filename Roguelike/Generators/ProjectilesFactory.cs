using Dungeons.Core;
using Roguelike.Abstract.Projectiles;
using Roguelike.Spells;

namespace Roguelike.Generators
{
  public class ProjectilesFactory : IProjectilesFactory
  {
    public IProjectile CreateProjectile(Vector2D pos, SpellKind sk)
    {
      return null;
    }
  }
}
