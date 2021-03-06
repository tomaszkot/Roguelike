using Dungeons.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Extensions
{
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
