using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dungeons.Core;
using Roguelike.Generators;

namespace Roguelike.Core.Crafting.Workers
{
  internal class OneEq : CraftWorker
  {
    internal OneEq(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.OneEq;
    }

    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = typeof(Equipment) };

      ri.Items.Add(i1);
      return ri;
    }

    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;
      if (!eqs.Any())
        previewResult = ReturnCraftingError("At least on equipment item is required");

      else if (!eqs.Any(i => CanBeCrafted(i)))
        previewResult = ReturnCraftingError("Can not craft equipment which was already crafted in pairs");

      
      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {
      var res = new List<Loot>();
      //var usedSrcItems = new List<Loot>();
      foreach (var srcEq in eqs)
      {
        if (!CanBeCrafted(srcEq))
          continue;
        var srcLootKind = srcEq.EquipmentKind;
        var lks = Equipment.GetPossibleLootKindsForCrafting().ToList();
        var destLk = RandHelper.GetRandomElem<EquipmentKind>(lks, new EquipmentKind[] { srcLootKind });
        var lootGenerator = lootCrafter.GetLootGenerator();
        var destEq = lootGenerator.GetRandomEquipment(destLk, srcEq.MinDropDungeonLevel, null);
        if (srcEq.Class == EquipmentClass.Magic)
        {
          destEq.SetClass(EquipmentClass.Magic, srcEq.MinDropDungeonLevel, null, srcEq.IsSecondMagicLevel);
        }
        var srcStatsCount = srcEq.GetMagicStats().Count;
        if (srcStatsCount > destEq.GetMagicStats().Count)
        {
          var diff = srcStatsCount - destEq.GetMagicStats().Count;
          for (int i = 0; i < diff; i++)
          {
            destEq.AddRandomMagicStat();
          }
        }
        if (srcEq.Enchantable)
        {
          destEq.MakeEnchantable();
        }
        destEq.WasCrafted = true;
        destEq.CraftingRecipe = RecipeKind.OneEq;
        res.Add(destEq);
        usedSrcItems.Add(srcEq);
      }
      return ReturnCraftedLoot(res, usedSrcItems);
    }

    private bool CanBeCrafted(Equipment srcEq)
    {
      return !srcEq.WasCraftedBy(RecipeKind.TwoEq);
    }

  }
}
