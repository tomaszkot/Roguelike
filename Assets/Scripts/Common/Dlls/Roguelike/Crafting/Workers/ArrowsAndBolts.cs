using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Crafting.Workers
{
  public class Arrows : CraftWorker
  {
    public Arrows()
    {
      Kind = RecipeKind.Arrows;
    }
    public Arrows(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.Arrows;
    }

    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;

      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {
     
      return ReturnCraftingError(InvalidIngredients);
    }

    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      return ri;
    }

  }

  public class Bolts : CraftWorker
  {
    public Bolts()
    {
      Kind = RecipeKind.Bolts;
    }

    public Bolts(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.Bolts;
    }

    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      return ri;
    }

    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;

      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {

      return ReturnCraftingError(InvalidIngredients);
    }
  }
}
