using Roguelike.Abstract.Spells;
using Roguelike.Core.Extensions;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Spells
{
  public class OffensiveSpell : Spell, IDamagingSpell
  {
    public static readonly List<int> AttackValueDecrease = Enumerable.Range(0, 20).ToList();

    float calcedDamage;
    public const int BaseDamage = 1;
    bool withVariation;
    public const int DefaultAddNominal = 0;
    public bool AlwaysHit { get; set; }

    public OffensiveSpell(LivingEntity caller, SpellKind sk, Weapon weaponSpellSource, bool withVariation = true) : base(caller, weaponSpellSource)
    {
      Kind = sk;
      this.withVariation = withVariation;
      var ale = caller as AdvancedLivingEntity;
      var en = caller as Enemy;

      if (en != null)
      {
        //TODO
        CurrentLevel = en.Level;
        
        if (en.PowerKind == EnemyPowerKind.Champion)
          CurrentLevel += 1;
        else if (en.PowerKind == EnemyPowerKind.Boss)
          CurrentLevel += 2;
      }
      else if (ale != null)
      {
        CurrentLevel = ale.Spells.GetState(Kind).Level;
      }
      else if (caller is LivingEntity le)
      {
        CurrentLevel = le.Level;
      }

      if (sk.IsGod())
      {
        CurrentLevel = caller.Level;
      }

      if (weaponSpellSource != null)
      {
        calcedDamage = CalcDamage(weaponSpellSource.LevelIndex, withVariation);
      }

    }

    protected virtual float CalcDamage(int spellLevel, bool withVariation)
    {
      float damage = BaseDamage;
      if (weaponSpellSource != null)
        damage += weaponSpellSource.LevelIndex;
      else
        damage += (spellLevel * 1.4f);

      //damage = damage - 1;

      float addNominal = DefaultAddNominal;

      damage += addNominal;
      if (damage <= 0)
        damage = 1;

      if(caller != null)//for proj. weapon it can be null
        damage += caller.GetExtraDamage(this.Kind, damage);

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

    public virtual string HitSound { get; }

    public float GetDamage()
    {
      //return withVariation ? Damage : NominalDamage;
      return Damage;
    }

    public override SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel)
    {
      bool withVariation = true;
      var desc = base.CreateSpellStatsDescription(currentMagicLevel);
      if (currentMagicLevel)
      {
        CalcDamage(CurrentLevel, withVariation);
        desc.Damage = GetDamage();
      }
      else
        desc.Damage = CalcDamage(CurrentLevel + 1, withVariation);
      return desc;
    }
  }

  
}
