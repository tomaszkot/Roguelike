using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
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
    public RecipeKind UsedKind { get; set; }
    public string Message { get; set; } = "";
    public List<Loot> OutLootItems { get; set; } = new List<Loot>();
    public List<Loot> UsedInputItems { get; set; } = new List<Loot>();


    public bool Success { get { return OutLootItems != null && OutLootItems.Any(); } }

    /// <summary>
    /// Normally true but in rare cases when Eq is enhanced/fixed (e.g.Magical weapon recharge) false
    /// </summary>
    public bool DeleteCraftedLoot { get; set; } = true;
    public bool AddOutLoot { get; set; }

    public CraftingResult(List<Loot> lootItems)
    {
      this.OutLootItems = lootItems;
    }

    public Loot FirstOrDefault()
    {
      return OutLootItems.FirstOrDefault();
    }

    public T FirstOrDefault<T>() where T : Loot
    {
      return OutLootItems.FirstOrDefault() as T;
    }

    public override string ToString()
    {
      return base.ToString() + ", "+Message;
    }
  }
}
