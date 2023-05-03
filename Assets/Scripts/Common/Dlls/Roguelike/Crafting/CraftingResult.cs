using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Crafting
{
  /// <summary>
  /// 
  /// </summary>
  public class CraftingResult
  {
    public string Message { get; set; }
    public List<Loot> LootItems { get; set; }

    public bool Success { get { return LootItems != null && LootItems.Any(); } }

    /// <summary>
    /// Normally true but in rare cases when Eq is enhanced/fixed (e.g.Magical weapon recharge) false
    /// </summary>
    public bool DeleteCraftedLoot { get; set; } = true;

    public CraftingResult(List<Loot> lootItems)
    {
      this.LootItems = lootItems;
    }

    public Loot FirstOrDefault()
    {
      return LootItems.FirstOrDefault();
    }

    public T FirstOrDefault<T>() where T : Loot
    {
      return LootItems.FirstOrDefault() as T;
    }
  }
}
