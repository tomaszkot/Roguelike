using Roguelike.Tiles;
using Roguelike.Tiles.Looting;

namespace Roguelike.Extensions
{
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

    public static bool IsPotion(this Loot loot, PotionKind kind)
    {
      var potion = loot as Potion;
      if (potion == null)
        return false;
      return potion.Kind == kind;
    }
       
  }
}
