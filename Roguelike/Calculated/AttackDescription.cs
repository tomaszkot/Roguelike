using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
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

    public AttackDescription(LivingEntity ent,
      bool withVariation = true,
      AttackKind attackKind = AttackKind.Unset,//if uset it will be based on current weapon/active fi
      OffensiveSpell spell = null)
    {
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
        var aent = ent as AdvancedLivingEntity;
        Weapon wpn = null;
        if (aent != null)//TODO add GetActiveWeapon in LivingEntity?
          wpn = aent.GetActiveWeapon();

        attackKind = DiscoverAttackKind(attackKind, wpn);

        if (attackKind == AttackKind.PhysicalProjectile)
        {
          if (ent.ActiveFightItem == null)
            return;
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
          if (ent.ActiveFightItem.IsBowLikeAmmo)
            Current += ent.ActiveFightItem.Damage;
          else
            Current -= wpn.Damage;//ammo not matching
        }
        else if (wpn.IsMagician && attackKind == AttackKind.WeaponElementalProjectile)
        {
          offensiveSpell = wpn.SpellSource.CreateSpell(ent) as OffensiveSpell;
        }
      }

      if (attackKind == AttackKind.PhysicalProjectile && ent.ActiveFightItem != null)
      {
        if (ent.ActiveFightItem.FightItemKind == Tiles.Looting.FightItemKind.Stone ||
           ent.ActiveFightItem.FightItemKind == Tiles.Looting.FightItemKind.ThrowingKnife
           )
        {
          Current += ent.ActiveFightItem.Damage;
          Current += ent.Stats.Strength/2;
        }
      }

      CurrentPhysical = Current;
      CurrentPhysicalVariated = CurrentPhysical;
      if (withVariation)//GUI is not meant to have it changed on character panel
      {
        var variation = ent.GetAttackVariation();
        var sign = RandHelper.Random.NextDouble() > .5f ? -1 : 1;

        CurrentPhysicalVariated += sign * variation * (float)RandHelper.Random.NextDouble();
      }

      if (CurrentPhysicalVariated < 0)
        CurrentPhysicalVariated = 0;
      if (CurrentPhysical < 0)
        CurrentPhysical = 0;

      var val = CurrentPhysical;
      AddExtraDamage(ent, wpn, weapons2Esk, ref val);
      CurrentPhysical = val;

      CurrentTotal = CurrentPhysical;
      var nonPhysical = ent.GetNonPhysicalDamages();

      if (offensiveSpell != null)
      {
        var dmg = offensiveSpell.GetDamage(withVariation);
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

    private void AddExtraDamage(LivingEntity ent, Weapon wpn, Dictionary<Weapon.WeaponKind, EntityStatKind> weapons2Esk, ref float currentDamage)
    {
      if (wpn != null && weapons2Esk != null)
      {
        if (weapons2Esk.ContainsKey(wpn.Kind))//AxeExtraDamage, SwordExtraDamage...
        {
          var extraPercentage = ent.Stats.GetCurrentValue(weapons2Esk[wpn.Kind]);
          currentDamage = FactorCalculator.AddFactor(currentDamage, extraPercentage);
        }
      }
    }
  }
}
