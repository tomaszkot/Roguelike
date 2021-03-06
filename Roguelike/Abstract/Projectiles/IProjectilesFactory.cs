using Roguelike.Abstract.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract.Projectiles
{
  public interface IProjectilesFactory
  {
    IProjectile CreateProjectile(Dungeons.Core.Vector2D pos);
  }
}
