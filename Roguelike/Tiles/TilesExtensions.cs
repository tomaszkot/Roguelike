using Dungeons.Tiles;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
  public static class TilesExtensions
  {
    public static bool IsDynamic(this Tile tile)
    {
      return tile is LivingEntity || tile is Tiles.Interactive.InteractiveTile || tile is Loot;
    }

    public static T As<T>(this Tile tile) where T : Tile
    {
      return tile as T;
    }
  }
}
