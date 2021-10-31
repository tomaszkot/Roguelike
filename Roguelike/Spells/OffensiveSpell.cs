using Dungeons.Core;
using Roguelike.Abstract.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;

namespace Roguelike.Spells
{
  public class OffensiveSpell : Spell, IDamagingSpell
  {
    float calcedDamage;
    public const int BaseDamage = 2;

    public OffensiveSpell(LivingEntity caller, Weapon weaponSpellSource) : base(caller, weaponSpellSource)
    {
      if (weaponSpellSource != null)
      {
        calcedDamage = CalcDamage(weaponSpellSource.LevelIndex);
      }
    }

    protected virtual float CalcDamage(int magicLevel)
    {
      //TODO
      //var dmg = damage + (damage * ((magicLevel - 1) * (damageMultiplicator + magicLevel * magicLevel / 2) / 100.0f));
      //return (float)Math.Ceiling(dmg);
      if (weaponSpellSource != null)
      {
        int add = 2;
        if (!weaponSpellSource.StableDamage)
        {
          var val = RandHelper.GetRandomDouble();
          if (val > 0.66f)
            add += 1;
          else if (val < 0.33f)
            add -= -1;
        }
        return weaponSpellSource.LevelIndex + BaseDamage;
      }
      return magicLevel + BaseDamage;
    }

    //Returns damage based on Spell level.
    //Spell level depends on the magic amount owner has.
    //For enemies magic amount is increased automatically as other stats.
    public float Damage
    {
      get
      {
        if (weaponSpellSource != null)
        {
          return calcedDamage;
        }
        var level = CurrentLevel;
        var dmg = CalcDamage(level);
        return dmg;
      }
    }

    public override SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel)
    {
      var desc = base.CreateSpellStatsDescription(currentMagicLevel);
      if(currentMagicLevel)
        desc.Damage = Damage;
      else
        desc.Damage = CalcDamage(CurrentLevel+1);
      return desc;
    }
  }

  
}
