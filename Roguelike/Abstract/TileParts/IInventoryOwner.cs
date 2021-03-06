using Roguelike.LootContainers;
using Roguelike.Tiles;

namespace Roguelike.Abstract.TileParts
{
  public interface IInventoryOwner
  {
    Roguelike.LootContainers.Inventory Inventory { get; set; }
    int GetPrice(Loot loot);
    int Gold { get; set; }
  }
}
