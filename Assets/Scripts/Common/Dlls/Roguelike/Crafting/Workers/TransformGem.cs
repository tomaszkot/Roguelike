using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System.Collections.Generic;
using Dungeons.Core;
using System.Linq;
using System.Diagnostics;

namespace Roguelike.Core.Crafting.Workers
{
  internal class TransformGem : CraftWorker
  {
    internal TransformGem(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.TransformGem;
    }
    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = typeof(Gem) };

      ri.Items.Add(i1);

      return ri;
    }


    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;
      var srcGems = Filter<Gem>(lootToConvert);
      if (srcGems.Count != 1)
        return ReturnCraftingError("One gem is required");

      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {
      var res = new List<Loot>();
      var srcGems = Filter<Gem>(lootToConvert);
      foreach (var srcGem in srcGems)
      {
        var destKind = RandHelper.GetRandomEnumValue<GemKind>(new GemKind[] { srcGem.GemKind, GemKind.Unset });
        var destGem = new Gem(destKind);
        destGem.EnchanterSize = srcGem.EnchanterSize;
        destGem.SetProps();
        res.Add(destGem);
        destGem.Count = srcGem.Count;
        Debug.WriteLine("Added gem: "+ destGem);
      }
      return ReturnCraftedLoot(res, srcGems.Cast<Loot>().ToList());
    }
  }
}
