using Dungeons.Tiles;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;

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
