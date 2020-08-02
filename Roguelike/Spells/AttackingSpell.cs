using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Spells
{
  public class OffensiveSpell : Spell
  {
    public OffensiveSpell() { }

    public OffensiveSpell(LivingEntity caller) : base(caller)
    {
    }
        
    //float damageMultiplicator = 45.0f;//%
  }

  public class ProjectiveSpell : OffensiveSpell
  {
    public const int BaseDamage = 2;
    public bool SourceOfDamage = true;

    //public AttackingSpell() : this(new LivingEntity()) { }
    public ProjectiveSpell(LivingEntity caller) : base(caller)
    {
      EntityRequired = true;
      EnemyRequired = true;
      damage = (caller is Enemy) ? BaseDamage - 1 : BaseDamage;
    }

    protected override void AppendPrivateFeatures(List<string> fe)
    {
      fe.Add("Damage: " + Damage);
    }

    protected override void AppendNextLevel(List<string> fe)
    {
      base.AppendNextLevel(fe);
      fe.Add("Next Level: Damage " + CalcDamage(GetCurrentLevel() + 1));
    }
  }
}
