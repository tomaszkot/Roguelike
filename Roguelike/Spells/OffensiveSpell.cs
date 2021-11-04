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
      var damage = BaseDamage;
      if (weaponSpellSource != null)
        damage += weaponSpellSource.LevelIndex;
      else
        damage += magicLevel;//TODO variation

      int add = 2;
      if (weaponSpellSource != null && !weaponSpellSource.StableDamage)
      {
        var val = RandHelper.GetRandomDouble();
        if (val > 0.66f)
          add += 1;
        else if (val < 0.33f)
          add -= -1;

        if (damage > 10)
          add *= 2;

        damage += add;
      }

      return damage;
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
