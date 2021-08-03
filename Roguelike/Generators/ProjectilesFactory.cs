using Dungeons.Core;
using Roguelike.Abstract.Projectiles;
using Roguelike.Spells;
using Roguelike.Tiles.Looting;

namespace Roguelike.Generators
{
  public class ProjectilesFactory : IProjectilesFactory
  {
    public IProjectile CreateProjectile(Vector2D pos, SpellKind sk)
    {
      return null;
    }

    public IProjectile CreateProjectile(Vector2D pos, FightItemKind fik)
    {
      throw new System.NotImplementedException();
    }
  }
}
