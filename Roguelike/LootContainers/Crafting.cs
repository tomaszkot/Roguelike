using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.LootContainers
{
  public class Crafting
  {
    public InventoryBase Recipes { get; set; }
    public InventoryBase InvItems { get; set; }

    public Crafting()
    {
      Recipes = new InventoryBase();
      Recipes.Capacity = 14;

      InvItems = new InventoryBase();
      Recipes.Capacity = 21;
    }
  }
}
