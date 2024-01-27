
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Crafting.Workers
{
  internal class EnchantEquipment : CraftWorker
  {
    Equipment toCraft;

    internal EnchantEquipment(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.EnchantEquipment;
 
    }

    public override void Init(List<Loot> lootToConvert, ILootCrafter lootCrafter)
    {
      base.Init(lootToConvert, lootCrafter);
      eqs = eqs.Where(i => i.Enchantable && (i.EnchantSlots - i.Enchants.Count) > 0).ToList();
    }

    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = typeof(Equipment) };
      var i4 = new RecipeRequiredItem() { Type = typeof(Enchanter) };
      ri.Items.Add(i1);
      ri.Items.Add(i4);
      return ri;
    }

    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;
      toCraft = eqs.FirstOrDefault();
      if (toCraft == null)
        previewResult = ReturnCraftingError("One enchantable piece of equipment is required by the recipe");
      else
      {
        //var nonEq = lootToConvert.Where(i => !(i is Equipment)).ToList();
        //if (nonEq.Any(i => !(i is Enchanter)))
        //{
        //  previewResult = ReturnCraftingError("Only enchanting items (gems, claws,...) are allowed by the recipe");
        //}
        //else
        {
          if (!enchanters.Any())
          {
            previewResult = ReturnCraftingError("Enchanting item is required");
          }
         // else
         // {
         //   if (freeSlots == 0)
         //     previewResult = ReturnCraftingError("No free slots available in the ");
         //}
        }
        
      }
      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {
      var eq = toCraft;
      string err;
      foreach (var ench in enchanters.Cast<Enchanter>())
      {
        if (!lootCrafter.ApplyEnchant(ench, eq, out err))
          return ReturnCraftingError(InvalidIngredients);
        else
          usedSrcItems.Add(ench);
      }

      return ReturnCraftedLoot(eq, usedSrcItems, false);

    }
  }
}
