using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Core.Crafting.Workers
{
  internal class ThreeGems : CraftWorker
  {
    Gem toCraft = null;

    internal ThreeGems(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.ThreeGems;
    }
    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = typeof(Gem) , MinCount = 3};
      ri.Items.Add(i1);

      return ri;
    }


    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;
      var gemsKinds = Filter<Gem>(lootToConvert).Where(i=>i.EnchanterSize != EnchanterSize.Big).GroupBy(i=>i.GemKind).Where(i=>i.Count() >=0);
      var notEnoughResources = "At least three gems of the same kind and size are required";
      if(gemsKinds.Count() == 0)
        return ReturnCraftingError(notEnoughResources);

      toCraft = null;
      foreach (var gemKind in gemsKinds)
      {
        var groupsBySize = gemKind.GroupBy(i => i.EnchanterSize).ToList();//.Where(i=>i.Count()>=3).FirstOrDefault();
        foreach (var groupBySize in groupsBySize)
        {
          foreach (var size in groupsBySize)
          {
            var si = size.ElementAt(0);
            if (si.Count >= 3)
            {
              toCraft = groupBySize.First();
              break;
            }
          }
          if (toCraft != null)
            break;
        }
        if (toCraft != null)
          break;
      }
      if(toCraft == null)
        return ReturnCraftingError(notEnoughResources);
      //    return ReturnCraftingError("Big gems can not be crafted");

      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {
      var gem = new Gem(toCraft.GameLevel);
      gem.GemKind = toCraft.GemKind;
      gem.EnchanterSize = toCraft.EnchanterSize == EnchanterSize.Small ? EnchanterSize.Medium : EnchanterSize.Big;
      gem.SetProps();
      gem.Count = toCraft.Count / 3;
      toCraft.Count = toCraft.Count % 3;

      if(toCraft.Count == 0)
        return ReturnCraftedLoot(gem, new List<Loot>() { toCraft });
      else
        return ReturnCraftedLoot(gem);
      

    }
  }
}
