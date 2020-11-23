using Roguelike.Abstract;
using Roguelike.Tiles;
using System.Collections.Generic;

namespace Roguelike.Spells
{
  public class OffensiveSpell : Spell, IDamagingSpell
  {
    protected float damage = 0;

    public OffensiveSpell() { }

    public OffensiveSpell(LivingEntity caller) : base(caller)
    {
    }

    protected virtual float CalcDamage(int magicLevel)
    {
      //TODO
      //var dmg = damage + (damage * ((magicLevel - 1) * (damageMultiplicator + magicLevel * magicLevel / 2) / 100.0f));
      //return (float)Math.Ceiling(dmg);
      return magicLevel+3;
    }

    //Returns damage based on Spell level.
    //Spell level depends on the magic amount owner has.
    //For enemies magic amount is increased automatically as other stats.
    public float Damage
    {
      get
      {
        var level = GetCurrentLevel();
        var dmg = CalcDamage(level);
        return dmg;
      }
    }

    protected override void AppendNextLevel(List<string> fe)
    {
      base.AppendNextLevel(fe);
      fe.Add(GetNextLevel("Damage " + CalcDamage(GetCurrentLevel() + 1)));
    }

    protected override void AppendPrivateFeatures(List<string> fe)
    {
      fe.Add("Damage: " + Damage);
    }
    //float damageMultiplicator = 45.0f;//%
  }

  public class ProjectiveSpell : OffensiveSpell
  {
    public const int BaseDamage = 2;
    public bool SourceOfDamage = true;

    public ProjectiveSpell(LivingEntity caller) : base(caller)
    {
      EntityRequired = true;
      EnemyRequired = true;
      damage = (caller is Enemy) ? BaseDamage - 1 : BaseDamage;
    }



  }
}
