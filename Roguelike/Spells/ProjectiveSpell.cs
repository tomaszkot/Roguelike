using Roguelike.Abstract.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Spells
{
  public class ProjectiveSpell : OffensiveSpell, IProjectileSpell
  {
    public const int BaseDamage = 4;
    public bool SourceOfDamage = true;
    public Dungeons.Tiles.IObstacle Target { get; set; }

    public ProjectiveSpell(LivingEntity caller, Weapon weapon) : base(caller, weapon)
    {
      EntityRequired = true;
      EnemyRequired = true;
    }
  }
}
