using Roguelike.LootContainers;
using Roguelike.Managers;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Core.Managers
{
  public class HeroInventoryManager
  {
    public GameManager GameManager { get; set; }
    public Hero Hero { get => GameManager.Hero; }
    public GameContext Context { get => GameManager.Context; }

    public List<Inventory> GetHeroInventories()
    {
      return new List<Inventory>() { Hero.Inventory, Hero.Crafting.InvItems.Inventory, Hero.Crafting.Recipes.Inventory };
    }
        
    public Recipe GetHeroRecipe(RecipeKind kind)
    {
      foreach (var inv in GetHeroInventories())
      {
        var rec = inv.GetRecipe(kind);
        if (rec != null)
          return rec;
      }
      return null;
    }

    public T GetStackedLootFromHeroInventory<T>() where T : StackedLoot
    {
      foreach (var inv in GetHeroInventories())
      {
        var loot = inv.GetStacked<T>().FirstOrDefault();
        if (loot != null)
          return loot;
      }
      return null;
    }
  }
}
