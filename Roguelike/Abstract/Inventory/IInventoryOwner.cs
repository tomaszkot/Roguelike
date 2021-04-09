using Roguelike.LootContainers;
using Roguelike.Tiles;

namespace Roguelike.Abstract.Inventory
{
  public interface IInventoryOwner
  {
    Roguelike.LootContainers.Inventory Inventory { get; }
    int GetPrice(Loot loot);
    int Gold { get; set; }
    bool GetGoldWhenSellingTo(IInventoryOwner other);
  }
}
