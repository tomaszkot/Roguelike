using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Core.Crafting.Workers
{
  internal class NiesiolowskiSoup : CraftWorker
  {
    Plant sorrel;
    Food plum;

    internal NiesiolowskiSoup(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.NiesiolowskiSoup;
    }

    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = typeof(Plant)};
      var i2 = new RecipeRequiredItem() { Type = typeof(Food) };
      ri.Items.Add(i1);
      ri.Items.Add(i2);
      return ri;
    }
    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;
      sorrel = Filter<Plant>(lootToConvert).Where(i => i.Kind == PlantKind.Sorrel).FirstOrDefault();
      if (sorrel == null)
        return ReturnCraftingError("Sorrel not available");

      plum = Filter<Food>(lootToConvert).Where(i => i.Kind == FoodKind.Plum).FirstOrDefault();
      if (plum == null)
        return ReturnCraftingError("Plum not available");
      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {
      var count = GetCraftedStackedCount(lootToConvert);
      usedSrcItems.Add(sorrel);
      usedSrcItems.Add(plum);
      //sorrel.Count -= count;
      //plum.Count -= count;
      return ReturnCraftedLoot(new Food() { Kind = FoodKind.NiesiolowskiSoup, Count = count }, usedSrcItems);
    }
  }
}
