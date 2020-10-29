using Newtonsoft.Json;
using SimpleInjector;
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
    Container container;
    public InvOwner InvOwner = InvOwner.Hero;

    [JsonIgnore]
    public Container Container 
    { 
      set 
      {
        container = value;
       // Recipes.Container = value;
       // InvItems.Container = value;
      } 
    }

    public Crafting(Container container)
    {
      Container = container;
      Recipes = new InventoryBase(container);
      Recipes.Capacity = 14;
      Recipes.InvOwner = InvOwner.Hero;
      Recipes.InvBasketKind = InvBasketKind.CraftingRecipe;

      InvItems = new InventoryBase(container);
      InvItems.Capacity = 21;
      InvItems.InvBasketKind = InvBasketKind.CraftingInvItems;

      InvOwner = InvOwner.Hero;
    }
  }
}
