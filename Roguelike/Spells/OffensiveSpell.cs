﻿using Dungeons.Core;
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
    public int NominalDamage { get; set; }
    bool withVariation;

    public OffensiveSpell(LivingEntity caller, Weapon weaponSpellSource, bool withVariation = true) : base(caller, weaponSpellSource)
    {
      this.withVariation = withVariation;
      if (weaponSpellSource != null)
      {
        calcedDamage = CalcDamage(weaponSpellSource.LevelIndex, withVariation);
      }
    }

    protected virtual float CalcDamage(int magicLevel, bool withVariation)
    {
      var damage = BaseDamage;
      if (weaponSpellSource != null)
        damage += weaponSpellSource.LevelIndex;
      else
        damage += magicLevel;//TODO variation
            
      int addNominal = 2;
      NominalDamage = damage + addNominal;
      if (withVariation)
      {
        if (weaponSpellSource != null && !weaponSpellSource.StableDamage)
        {
          var val = RandHelper.GetRandomDouble();
          if (val > 0.66f)
            addNominal += 1;
          else if (val < 0.33f)
            addNominal -= -1;

          if (damage > 10)
            addNominal *= 2;
        }
      }
      damage += addNominal;
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
        var dmg = CalcDamage(level, withVariation);
        return dmg;
      }
    }

    public override SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel)
    {
      var desc = base.CreateSpellStatsDescription(currentMagicLevel);
      if(currentMagicLevel)
        desc.Damage = Damage;
      else
        desc.Damage = CalcDamage(CurrentLevel+1, withVariation);
      return desc;
    }
  }

  
}
