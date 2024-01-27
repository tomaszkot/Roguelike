using Dungeons.Core;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
      OffensiveSpell spell = null,
      ProjectileFightItem pfi = null)
    {
      AttackKind = attackKind;
      fightItem = null;
      Calc(ent, withVariation, ref attackKind, spell, pfi);
    }

    private void Calc(LivingEntity ent, bool withVariation, ref AttackKind attackKind, 
                      OffensiveSpell spell, ProjectileFightItem pfi = null)
    {
      try
      {
        this.withVariation = withVariation;
        this.spell = spell;
        NonPhysical = new Dictionary<EntityStatKind, float>();
        this.ent = ent;
        Weapon wpn = ent.GetActiveWeapon();
        //if (ent != null)//can be if using WeaponElementalProjectile
        {
          if (attackKind == AttackKind.Unset)
          {
            
            attackKind = DiscoverAttackKind(attackKind, wpn);
          }
          AttackKind = attackKind;
        }

        if (AttackKind == AttackKind.SpellElementalProjectile && spell == null)
          return;

        FightItem fi = null;
        if (attackKind == AttackKind.PhysicalProjectile)
        {
          fightItem = GetActiveFightItem(ent);
          fi = fightItem;
          if (fightItem == null)
          {
            fightItem = pfi;
            if (fightItem == null)
              return;
          }
        }
        if (attackKind == AttackKind.WeaponElementalProjectile)
        {
          if (wpn != null)
          {
            if (wpn.SpellSource == null || wpn.SpellSource.Count == 0)
              return;
          }
          else
            return;
        }

        Dictionary<Weapon.WeaponKind, EntityStatKind> weapons2Esk = null;
        EntityStatKind attackStat = EntityStatKind.Unset;

        if (fi != null && fi.FightItemKind == FightItemKind.Smoke)
          return;

        if (fi != null && 
          (fi.FightItemKind == FightItemKind.CannonBall ||
          fi.FightItemKind == FightItemKind.Smoke)
          )
        { 
        }
        else if (attackKind == AttackKind.Melee)
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
    }
        

    public static FightItem GetActiveFightItem(LivingEntity ent)
    {
      var fightItem = ent.GetActivatedFightItem();
      if (fightItem == null)
        fightItem = ent.GetFightItemFromActiveProjectileAbility();

      return fightItem;
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
      if(attackStat != EntityStatKind.Unset)
        Current = ent.GetCurrentValue(attackStat);
      else
        Current = 0;
      OffensiveSpell offensiveSpell = spell;
      if (wpn != null)
      {
        if (wpn.IsBowLike && attackKind == AttackKind.PhysicalProjectile)
        {
          if (!fightItem.IsBowLikeAmmo)
            Current -= wpn.Damage;//ammo not matching
        }
        else if (wpn.IsMagician && attackKind == AttackKind.WeaponElementalProjectile)
        {
          offensiveSpell = wpn.SpellSource.CreateSpell(ent) as OffensiveSpell;
        }
      }

      if (attackKind == AttackKind.PhysicalProjectile && fightItem != null)
      {
        //bool addfightItemDamage = false;
        if (fightItem.FightItemKind == Tiles.Looting.FightItemKind.Stone ||
           fightItem.FightItemKind == Tiles.Looting.FightItemKind.ThrowingKnife ||
           fightItem.FightItemKind == Tiles.Looting.FightItemKind.ThrowingTorch
           )
        {
          Current += ent.Stats.Strength/2;
          //addfightItemDamage = true;
        }

        //if (fightItem.FightItemKind == Tiles.Looting.FightItemKind.CannonBall)
        //  addfightItemDamage = true;

        //if(addfightItemDamage)//trap did 0 damage to skeleton (he is resist on bleeding!)
          Current += fightItem.Damage;
        //else if(!fightItem.IsBowLikeAmmo)//done above
          //Debug.WriteLine("! error PhysicalProjectile damage not calced!");
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
      //add melee damage
      AddExtraDamage(ent, wpn, weapons2Esk, ref val);
      CurrentPhysicalVariated += val - CurrentPhysicalVariated;
      
      CurrentTotal = CurrentPhysicalVariated;
      
      if (offensiveSpell != null && 
         (attackKind == AttackKind.SpellElementalProjectile || attackKind == AttackKind.WeaponElementalProjectile))
      {
        var dmg = offensiveSpell.GetDamage();
        if(withVariation)
          dmg += CalcVariation(attackKind, true, dmg);
        if (wpn != null && attackKind == AttackKind.WeaponElementalProjectile)
        {
          //add extra magic projectile damage
          AddExtraDamage(ent, wpn, weapons2Esk, ref dmg);
        }

        NonPhysical[offensiveSpell.Kind.ToEntityStatKind()] = dmg;
        CurrentTotal += dmg;
      }

      if (attackKind != AttackKind.SpellElementalProjectile)
      {
        var nonPhysical = ent.GetNonPhysicalDamages();
        foreach (var npd in nonPhysical)
        {
          bool add = true;
          if (wpn != null)
          {
            if (attackKind == AttackKind.WeaponElementalProjectile)
            {
              if (wpn.IsMagician)
                add = offensiveSpell.Kind.ToEntityStatKind() == npd.Key;
            }
            else if (attackKind == AttackKind.Melee)
              add = !wpn.IsBowLike;

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
      if (wpn != null)
      {
        if (weapons2Esk != null)
        {
          if (weapons2Esk.ContainsKey(wpn.Kind))//AxeExtraDamage, SwordExtraDamage...
          {
            currentDamage = ent.Stats.GetStat(weapons2Esk[wpn.Kind]).SumPercentageFactorAndValue(currentDamage);
          }
        }
        if (ent is AdvancedLivingEntity ale)
          ApplyAbility(ref currentDamage, wpn, ale);
      }
    }

    Ability GetAbility(Weapon wpn, AdvancedLivingEntity ale) 
    {
      if (wpn.IsBowLike)
      {
        var ab = ale.GetActivePhysicalProjectileAbility();
        if (ab != null)
        {
          if (ab.Kind != AbilityKind.PerfectHit)
            return null;
          return ab;
        }
      }
      else
      {
        //if (ale.CanUseAbility(AbilityKind.Rage))
        //{
        //  return ale.GetActiveAbility(AbilityKind.Rage);
        //}
      }
      return null;
    }

    private void ApplyAbility(ref float currentDamage, Weapon wpn, AdvancedLivingEntity ale)
    {
      var ability = GetAbility(wpn, ale);
      string reason;
      if (ability == null || !ale.CanUseAbility(ability.Kind, null, out reason))
        return;
      if (wpn.IsBowLike)
      {
        if (ability.Kind == AbilityKind.PerfectHit)
        {
          var nd = ability.PrimaryStat.SumPercentageFactorAndValue(currentDamage);
          //if (nd / currentDamage < 1.5)
          //{
          //  int k = 0;
          //  k++;
          //}
          currentDamage = nd;
        }
      }
      else
      {
        //if (AbilityKind.Rage == ability.Kind)
        //  currentDamage = FactorCalculator.AddFactor(currentDamage, ale.SelectedActiveAbility.PrimaryStat.Factor);
        //else 
        if (AbilityKind.ElementalVengeance == ability.Kind)
        {
          //currentDamage = FactorCalculator.AddFactor(currentDamage, ale.SelectedActiveAbility.PrimaryStat.Factor);
        }
      }
    }

    public override string ToString()
    {
      return base.ToString() + ", CurrentTotal: " + CurrentTotal;
    }
  }
}
