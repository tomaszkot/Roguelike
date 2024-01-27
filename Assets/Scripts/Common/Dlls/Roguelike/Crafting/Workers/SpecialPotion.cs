using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roguelike.Extensions;
using Roguelike.Crafting.Workers;
using Roguelike.Probability;

namespace Roguelike.Core.Crafting.Workers
{
  internal class SpecialPotion : CraftWorker
  {
    StackedLoot hps;
    StackedLoot mps;
    StackedLoot toads;
    internal SpecialPotion(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.CraftSpecialPotion;
    }

    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = typeof(Potion) };
      var i2 = new RecipeRequiredItem() { Type = typeof(Mushroom) };
      ri.Items.Add(i1);
      ri.Items.Add(i2);
      return ri;
    }

    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;
      hps = stacked.Where(i => i.IsPotionKind(PotionKind.Health)).FirstOrDefault();
      mps = stacked.Where(i => i.IsPotionKind(PotionKind.Mana)).FirstOrDefault();
      if (hps == null && mps == null)
        previewResult = ReturnCraftingError("At least one Mana or Health potion is required");
      else
      {
        toads = stacked.Where(i => i.IsBoletus()).FirstOrDefault();
        if (toads == null)
          previewResult = ReturnCraftingError("At least one Boletus is required");
      }
      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {
      Potion pot = null;
      var maxLoot = GetMaxStackedCount(lootToConvert.Where(i=>i.IsPotion()).ToArray());
      if (maxLoot == hps)
      {
        var count = GetCraftedStackedCount(new Loot[] { hps, toads });
        pot = new Roguelike.Tiles.Looting.SpecialPotion(SpecialPotionKind.Strength, SpecialPotionSize.Small) { Count = count };
        usedSrcItems.Add(hps);
      }
      else
      {
        var count = GetCraftedStackedCount(new Loot[] { mps, toads });
        usedSrcItems.Add(mps);
        pot = new Roguelike.Tiles.Looting.SpecialPotion(SpecialPotionKind.Magic, SpecialPotionSize.Small) { Count = count };
      }
      usedSrcItems.Add(toads);
      return ReturnCraftedLoot(pot, usedSrcItems);
    }
  }
}
