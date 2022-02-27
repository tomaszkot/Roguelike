using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System.Collections.Generic;

namespace Roguelike.Calculated
{
  public class AttackDescription
  {
    public float Nominal { get; set; }//strength
    float Current { get; set; }//strength + weapon melee damage
    public float CurrentPhysical { get; set; }//Current + Extra form abilities
    public float CurrentPhysicalVariated { get; set; }
    public Dictionary<EntityStatKind, float> NonPhysical { get; private set; }//weapon  ice, fire... damages
    public float CurrentTotal { get; set; }//Current + NonPhysical

    public string Display { get; set; }
    LivingEntity ent = null;
    OffensiveSpell spell;
    bool withVariation;
    FightItem fightItem;
    public AttackKind AttackKind { get; private set; }

    public AttackDescription(LivingEntity ent,
      bool withVariation = true,
      AttackKind attackKind = AttackKind.Unset,//if uset it will be based on current weapon/active fi
      OffensiveSpell spell = null)
    {
      AttackKind = attackKind;
      fightItem = null;
      Calc(ent, withVariation, ref attackKind, spell);
    }

    private void Calc(LivingEntity ent, bool withVariation, ref AttackKind attackKind, OffensiveSpell spell)
    {
      try
      {
        this.withVariation = withVariation;
        this.spell = spell;
        NonPhysical = new Dictionary<EntityStatKind, float>();
        this.ent = ent;
        Weapon wpn = null;
        //if (ent != null)//can be if using WeaponElementalProjectile
        {
          wpn = ent.GetActiveWeapon();

          attackKind = DiscoverAttackKind(attackKind, wpn);
          AttackKind = attackKind;
        }
        //else
        //  attackKind = AttackKind.WeaponElementalProjectile;

        if (attackKind == AttackKind.PhysicalProjectile)
        {
          fightItem = ent.ActiveFightItem;
          if (fightItem == null)
          {
            fightItem = ent.RecentlyActivatedFightItem;//HACK
            if (fightItem == null)
              return;
          }
        }
        if (attackKind == AttackKind.WeaponElementalProjectile)
        {
          //if (wpn != null && )
          //{
          //  if (wpn.SpellSource.Count == 0)
          //    return;
          //}
        }

        Dictionary<Weapon.WeaponKind, EntityStatKind> weapons2Esk = null;
        EntityStatKind attackStat = EntityStatKind.Unset;
        if (attackKind == AttackKind.Melee)
        {
          attackStat = EntityStatKind.MeleeAttack;

          weapons2Esk = AdvancedLivingEntity.MalleeWeapons2Esk;
          if (wpn != null && wpn.IsMagician)
            weapons2Esk = AdvancedLivingEntity.ProjectileWeapons2Esk;

        }
        else if (attackKind == AttackKind.PhysicalProjectile ||
          attackKind == AttackKind.WeaponElementalProjectile)
        {
          attackStat = EntityStatKind.Unset;
          if (attackKind == AttackKind.PhysicalProjectile)
            attackStat = EntityStatKind.PhysicalProjectilesAttack;
          else
            attackStat = EntityStatKind.ElementalWeaponProjectilesAttack;
          weapons2Esk = AdvancedLivingEntity.ProjectileWeapons2Esk;
        }
        CalcMembers(ent, wpn, weapons2Esk, attackStat, attackKind);
      }
      catch (System.Exception ex)
      {
        throw ex;
      }
      //finally
      //{
      //  if (CurrentTotal == 0 && attackKind != AttackKind.Melee)
      //  {
      //    AttackKind attackKindAlt = AttackKind.Melee;
      //    Calc(ent, withVariation, ref attackKindAlt, spell);
      //  }
      //}
    }

    public static AttackKind DiscoverAttackKind(AttackKind attackKind, Weapon wpn)
    {
      if (attackKind == AttackKind.Unset)
      {
        attackKind = AttackKind.Melee;
        if (wpn != null)
        {
          if (wpn.IsBowLike)
            attackKind = AttackKind.PhysicalProjectile;
          else if (wpn.IsMagician)
            attackKind = AttackKind.WeaponElementalProjectile;
        }
      }

      return attackKind;
    }

    private void CalcMembers
    (
      LivingEntity ent, 
      Weapon wpn, 
      Dictionary<Weapon.WeaponKind, EntityStatKind> weapons2Esk, 
      EntityStatKind attackStat, 
      AttackKind attackKind
    )
    {
      Current = ent.GetCurrentValue(attackStat);
      OffensiveSpell offensiveSpell = spell;
      if (wpn != null)
      {
        if (wpn.IsBowLike && attackKind == AttackKind.PhysicalProjectile)
        {
          if (fightItem.IsBowLikeAmmo)
            Current += fightItem.Damage;
          else
            Current -= wpn.Damage;//ammo not matching
        }
        else if (wpn.IsMagician && attackKind == AttackKind.WeaponElementalProjectile)
        {
          offensiveSpell = wpn.SpellSource.CreateSpell(ent) as OffensiveSpell;
        }
      }

      if (attackKind == AttackKind.PhysicalProjectile && fightItem != null)
      {
        if (fightItem.FightItemKind == Tiles.Looting.FightItemKind.Stone ||
           fightItem.FightItemKind == Tiles.Looting.FightItemKind.ThrowingKnife
           )
        {
          Current += fightItem.Damage;
          Current += ent.Stats.Strength/2;
        }
      }

      CurrentPhysical = Current;
      CurrentPhysicalVariated = CurrentPhysical;
      if (withVariation)//GUI is not meant to have it changed on character panel
      {
        CurrentPhysicalVariated += CalcVariation(attackKind, true, CurrentPhysical);
      }

      if (CurrentPhysicalVariated < 0)
        CurrentPhysicalVariated = 0;
      if (CurrentPhysical < 0)
        CurrentPhysical = 0;

      var val = CurrentPhysicalVariated;
      AddExtraDamage(ent, wpn, weapons2Esk, ref val);
      CurrentPhysicalVariated += val - CurrentPhysicalVariated;
      
      CurrentTotal = CurrentPhysicalVariated;
      var nonPhysical = ent.GetNonPhysicalDamages();

      if (offensiveSpell != null)
      {
        var dmg = offensiveSpell.GetDamage();
        dmg += CalcVariation(attackKind, true, dmg);
        if (wpn != null && attackKind == AttackKind.WeaponElementalProjectile)
        {
          AddExtraDamage(ent, wpn, weapons2Esk, ref dmg);
        }
        NonPhysical[offensiveSpell.Kind.ToEntityStatKind()] = dmg;
        CurrentTotal += dmg;
      }

      foreach (var npd in nonPhysical)
      {
        bool add = true;
        if (wpn != null)
        {
          if (wpn.IsMagician && attackKind == AttackKind.WeaponElementalProjectile)
          {
            add = offensiveSpell.Kind.ToEntityStatKind() == npd.Key;
          }
          if (wpn.IsBowLike && attackKind == AttackKind.Melee)
            add = false;
        }
        if (add)
        {
          var addition = npd.Value;
          AddExtraDamage(ent, wpn, weapons2Esk, ref addition);
          CurrentTotal += addition;


          if (!NonPhysical.ContainsKey(npd.Key))
            NonPhysical[npd.Key] = 0;
          NonPhysical[npd.Key] += addition;
        }
      }

      Nominal = ent.Stats.GetStat(attackStat).Value.Nominal;
      Display = Nominal + "/" + CurrentTotal;
    }

    public float CalcVariation(AttackKind attackKind, bool signed, float currentDamage)
    {
      return ent.GetAttackVariation(attackKind, currentDamage, signed);
    }

    private void AddExtraDamage(LivingEntity ent, Weapon wpn, Dictionary<Weapon.WeaponKind, EntityStatKind> weapons2Esk, ref float currentDamage)
    {
      if (wpn != null && weapons2Esk != null)
      {
        if (weapons2Esk.ContainsKey(wpn.Kind))//AxeExtraDamage, SwordExtraDamage...
        {
          currentDamage = ent.Stats.GetStat(weapons2Esk[wpn.Kind]).SumPercentageFactorAndValue(currentDamage);
          //var extraPercentage = ent.Stats.GetCurrentValue(weapons2Esk[wpn.Kind]);
          //var currentDamage1 = FactorCalculator.AddFactor(currentDamage, extraPercentage);
          //if (currentDamage1 != currentDamage)
          //  throw new System.Exception("currentDamage1 != currentDamage");
        }
      }
    }
  }
}
