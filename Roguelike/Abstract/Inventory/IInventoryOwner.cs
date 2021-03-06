using Roguelike.LootContainers;
using Roguelike.Tiles;

namespace Roguelike.Abstract
{
  public interface IInventoryOwner
  {
    Inventory Inventory { get; set; }
    int GetPrice(Loot loot);
    int Gold { get; set; }
  }
}
