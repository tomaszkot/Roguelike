using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Core.Crafting.Workers
{
  internal class UnEnchantEquipment : CraftWorker
  {
    Equipment eq;
    internal UnEnchantEquipment(List<Loot> lootToConvert, ILootCrafter lootCrafter) : 
      base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.UnEnchantEquipment;
    }

    public override void Init(List<Loot> lootToConvert, ILootCrafter lootCrafter)
    {
      base.Init(lootToConvert, lootCrafter);
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
      if (eqs.Count != 1 || eqs[0].Enchants.Count == 0)
        return GetError("One enchanted piece of equipment is required");
      else if(lootToConvert.Any(i => !(i is Equipment) ))
        return GetError("Only equipment is allowed for this recipe");

      eq = eqs[0];
      return ReturnCanDo(previewResult);
    }

    private CraftingResult GetError(string err)
    {
      return ReturnCraftingError(err);
    }


    public override CraftingResult Do()
    {
      var enchs = eq.Enchants.Select(i => i.Enchanter).ToList();
      enchs.ForEach(i => i.Count = 1);
      foreach (var ench in eq.Enchants)
      {
        foreach (var stat in ench.StatKinds)
        {
          eq.RemoveMagicStat(stat, ench.StatValue);
        }
      }
      eq.Enchants = new List<Enchant>();

      var lootItems = new List<Loot>() { eq };
      lootItems.AddRange(enchs);
      usedSrcItems = new List<Loot>();//nothing to remove
      var res = ReturnCraftedLoot(lootItems, usedSrcItems, false);
      res.AddOutLoot = true;
      return res;
    }
  }
}
