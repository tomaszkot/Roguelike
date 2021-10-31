using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Calculated
{
  public class AttackDescription
  {
    public float Nominal { get; set; }//strength
    float Current { get; set; }//strength + weapon melee damage
    public float CurrentPhysical { get; set; }//Current + Extra form abilities
    Dictionary<EntityStatKind, float> NonPhysical { get; set; }//weapon  ice, fire... damages
    public float CurrentTotal { get; set; }//Current + NonPhysical

    public string Display { get; set; }

    public AttackDescription(LivingEntity ent, AttackKind attackKind = AttackKind.Unset)//if uset it will be based on current weapon/active fi
    {
      var aent = ent as AdvancedLivingEntity;
      Weapon wpn = null;
      if (aent != null)//TODO add GetActiveWeapon in LivingEntity?
        wpn = aent.GetActiveWeapon();

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

      if (attackKind == AttackKind.PhysicalProjectile)
      {
        if (ent.ActiveFightItem == null)
          return;
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

    private void CalcMembers(LivingEntity ent, Weapon wpn, Dictionary<Weapon.WeaponKind, EntityStatKind> weapons2Esk, 
      EntityStatKind attackStat, AttackKind attackKind)
    {
      Current = ent.GetCurrentValue(attackStat);
      OffensiveSpell offensiveSpell = null;
      if (wpn !=null)
      {
        if (wpn.IsBowLike && attackKind == AttackKind.PhysicalProjectile)
        {
          Current += ent.ActiveFightItem.Damage;
          //Current += wpn.Damage; added by ent.GetCurrentValue(attackStat)
        }
        else if (wpn.IsMagician && attackKind == AttackKind.WeaponElementalProjectile)
        {
          offensiveSpell = wpn.SpellSource.CreateSpell(ent) as OffensiveSpell;
          if (offensiveSpell != null)
          {
            Current += offensiveSpell.Damage;
          }
        }
      }

      if (attackKind == AttackKind.PhysicalProjectile && ent.ActiveFightItem !=null)
      {
        if (ent.ActiveFightItem.FightItemKind == Tiles.Looting.FightItemKind.Stone ||
           ent.ActiveFightItem.FightItemKind == Tiles.Looting.FightItemKind.ThrowingKnife)
        {
          Current += ent.ActiveFightItem.Damage;
          Current += ent.Stats.Strength;
        }
      }

      CurrentPhysical = Current;

      if (wpn != null && weapons2Esk !=null)
      {
        if (weapons2Esk.ContainsKey(wpn.Kind))//AxeExtraDamage, SwordExtraDamage...
        {
          var extraPercentage = ent.Stats.GetCurrentValue(weapons2Esk[wpn.Kind]);
          CurrentPhysical = FactorCalculator.AddFactor(CurrentPhysical, extraPercentage);
        }
      }

      CurrentTotal = CurrentPhysical;
      NonPhysical = ent.GetNonPhysicalDamages();
      foreach (var npd in NonPhysical)
      {
        bool add = true;
        if (wpn.IsMagician && attackKind == AttackKind.WeaponElementalProjectile)
        {
          add = offensiveSpell.Kind.ToEntityStatKind() == npd.Key;
        }
        if (wpn.IsBowLike && attackKind == AttackKind.Melee)
          add = false;
        if (add)
          CurrentTotal += npd.Value;
      }
      Nominal = ent.Stats.GetStat(attackStat).Value.Nominal;

      Display = Nominal + "/" + CurrentTotal;
    }
  }
}
