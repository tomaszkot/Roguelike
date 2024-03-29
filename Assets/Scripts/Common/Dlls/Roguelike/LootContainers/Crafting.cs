﻿using Newtonsoft.Json;
using Roguelike.Abstract.Inventory;
using Roguelike.Tiles;
using SimpleInjector;
using System;

namespace Roguelike.LootContainers
{
  public class InventoryOwner : IInventoryOwner
  {
    public Inventory Inventory { get; set; }
    public int Gold { get; set; }

    public virtual bool GetGoldWhenSellingTo(IInventoryOwner other)
    {
      return false;
    }

    public int GetPrice(Loot loot)
    {
      throw new NotImplementedException();
    }

    public bool IsSellable(Loot loot)
    {
      return loot.IsSellable();
    }
  }

  public class Crafting
  {
    public InventoryOwner Recipes { get; set; }
    public InventoryOwner InvItems { get; set; }
    Container container;
    public InvOwner InvOwner = InvOwner.Hero;

    [JsonIgnore]
    public Container Container
    {
      set
      {
        container = value;
        Recipes.Inventory.Container = value;
        InvItems.Inventory.Container = value;
      }
    }

    public Crafting(Container container)
    {
      var recipes = new Inventory(container);
      recipes.Capacity = 14;
      //recipes.InvOwner = InvOwner.Hero;
      recipes.InvBasketKind = InvBasketKind.CraftingRecipe;

      Recipes = new InventoryOwner();
      Recipes.Inventory = recipes;


      var invItems = new Inventory(container);
      invItems.Capacity = 21;
      invItems.InvBasketKind = InvBasketKind.CraftingInvItems;
      InvItems = new InventoryOwner();
      InvItems.Inventory = invItems;

      InvOwner = InvOwner.Hero;
      Container = container;
    }
  }
}
