using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Extensions;

namespace Roguelike.Core.Crafting.Workers
{
  internal class Toadstools2Potion : CraftWorker
  {
    int srcCount = 0;
    const int countReq = 3;
    internal Toadstools2Potion(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.Toadstools2Potion;
    }
    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = typeof(Mushroom), MinCount = 3 };
      ri.Items.Add(i1);
      return ri;
    }

    Mushroom fromCanDo;
    public override CraftingResult CanDo()
    {
      var toadstools = lootToConvert.Where(i => i.IsToadstool()).Cast<Mushroom>().GroupBy(i=>i.MushroomKind).ToList();
      int matches = 0;
      
      foreach (var ts in toadstools)
      {
        var mushOfKind = ts.ElementAt(0);
        if (mushOfKind.Count >= countReq)
        {
          matches++;
          fromCanDo = mushOfKind;
          break;
        }

      }
      //var matches = toadstools.Where(i=>i.Count() >= 3).ToList();
      if(matches == 0)
        return ReturnCraftingError("At least three toadstools (of the same kind) are required");
      //;///<StackedLoot>(lootToConvert, "Sulfur");
      //if (allToadstool)
      //{
      //  var toadstool = lootToConvert[0].AsToadstool();
      //  if (toadstool == null || toadstool.Count < 3)
      //  {
      //    return ReturnCraftingError("At least three toadstools are required");
      //  }
      //}
      //else
      //  return ReturnCraftingError("Only toadstools are allowed");

      srcCount = fromCanDo.Count;
      
      return new CraftingResult(lootToConvert) { UsedInputItems = new List<Loot>() { fromCanDo } };
    }

    public override CraftingResult Do()
    {
      fromCanDo.Count = fromCanDo.Count % countReq;// (fromCanDo.Count/ countReq)* countReq;
      var toadstool = fromCanDo;
      Potion potion = null;
      if (toadstool.MushroomKind == MushroomKind.BlueToadstool)
        potion = new Potion(PotionKind.Mana) { Count = srcCount/ countReq };
      else
        potion = new Potion(PotionKind.Health) { Count = srcCount/ countReq };

      var src = new List<Loot>() { toadstool };
      var res = ReturnCraftedLoot(potion, src, false);
      return res;
    }
  }
}
