using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;

namespace Roguelike.Extensions
{
  public static class StatsExtensions
  {
    public static bool IsThrowingTorch(this Loot loot)
    {
      return loot is ProjectileFightItem pfi && pfi.FightItemKind == FightItemKind.ThrowingTorch;
    }

    public static AttackKind ToAttackKind(this EntityStatKind esk)
    {
      if (esk == EntityStatKind.MeleeAttack)
        return AttackKind.Melee;
      else if (esk == EntityStatKind.PhysicalProjectilesAttack)
        return AttackKind.PhysicalProjectile;
      else if (esk == EntityStatKind.ElementalWeaponProjectilesAttack)
        return AttackKind.WeaponElementalProjectile;
      else if (esk == EntityStatKind.ElementalSpellProjectilesAttack)
        return AttackKind.SpellElementalProjectile;

      return AttackKind.Unset;
    }

    public static bool IsAttackStat(this EntityStatKind esk)
    {
      return esk == EntityStatKind.MeleeAttack || esk == EntityStatKind.PhysicalProjectilesAttack ||
             esk == EntityStatKind.ElementalSpellProjectilesAttack || esk == EntityStatKind.ElementalWeaponProjectilesAttack;
    }

    public static bool IsChanceToHitStat(this EntityStatKind esk)
    {
      return esk == EntityStatKind.ChanceToMeleeHit || esk == EntityStatKind.ChanceToPhysicalProjectileHit;
    }
  }

  public static class EquipmentExtensions
  {
    public static CurrentEquipmentKind GetCurrentEquipmentKind(this EquipmentKind ek, CurrentEquipmentPosition pos)
    {
      return Equipment.FromEquipmentKind(ek, pos);
    }

    public static CurrentEquipmentPosition GetCurrentEquipmentPosition(this CurrentEquipmentKind currentEquipmentKind)
    {
      var pos = CurrentEquipmentPosition.Left;
      if (currentEquipmentKind == CurrentEquipmentKind.RingRight)
      {
        pos = CurrentEquipmentPosition.Right;
      }
      if (currentEquipmentKind == CurrentEquipmentKind.TrophyRight)
      {
        pos = CurrentEquipmentPosition.Right;
      }
      return pos;
    }

    public static EquipmentKind GetEquipmentKind(this CurrentEquipmentKind cek)
    {
      CurrentEquipmentPosition pos1;
      return Equipment.FromCurrentEquipmentKind(cek, out pos1);
    }
  }

  public static class LootExtensions
  {

    public static Mushroom AsToadstool(this Loot loot)
    {
      var mash = loot as Mushroom;
      if (mash == null)
        return null;
      if (mash.MushroomKind == MushroomKind.BlueToadstool || mash.MushroomKind == MushroomKind.RedToadstool)
        return mash;

      return null;
    }

    public static bool IsToadstool(this Loot loot)
    {
      var mash = loot as Mushroom;
      if (mash == null)
        return false;
      return mash.MushroomKind == MushroomKind.BlueToadstool || mash.MushroomKind == MushroomKind.RedToadstool;
    }

    public static bool IsPotion(this Loot loot)
    {
      var potion = loot as Potion;
      return potion != null;
    }

    public static bool IsPotionKind(this Loot loot, PotionKind kind)
    {
      var potion = loot as Potion;
      if (potion == null)
        return false;
      return potion.Kind == kind;
    }
    public static Potion AsPotionKind(this Loot loot, PotionKind kind)
    {
      var potion = loot as Potion;
      if (potion == null)
        return null;
      return potion.Kind == kind ? loot as Potion : null;
    }

    public static Potion AsPotion(this Loot loot)
    {
      return loot as Potion;
    }

    public static bool IsBowLikeAmmunition(this FightItemKind fightItemKind)
    {
      return Weapon.IsBowLikeAmmoKind(fightItemKind);
    }

    public static bool IsBowAmmoKind(this FightItemKind kind)
    {
      return Weapon.IsBowAmmoKind(kind);
    }

    public static bool IsCrossBowAmmoKind(this FightItemKind kind)
    {
      return Weapon.IsCrossBowAmmoKind(kind);
    }

  }
}
