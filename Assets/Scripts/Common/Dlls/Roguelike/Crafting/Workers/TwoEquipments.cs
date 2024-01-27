using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Core.Crafting.Workers
{
  internal class TwoEquipments : CraftWorker
  {
    internal TwoEquipments(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.TwoEq;
    }

    public override void Init(List<Loot> lootToConvert, ILootCrafter lootCrafter)
    {
      base.Init(lootToConvert, lootCrafter);
      //TODO use code from CanDo()
      //eqs = eqs.Where(i => i.WasCrafted && i.Enchants.Count > 0).ToList();
    }

    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = typeof(Equipment), MinCount = 1 };
      var i2 = new RecipeRequiredItem() { Type = typeof(Equipment), MinCount = 1 };
      ri.Items.Add(i1);
      ri.Items.Add(i2);
      return ri;
    }


    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;
      if (eqs.Count == 2)
      {
        var eq1 = eqs[0];
        var eq2 = eqs[1];
        if (eq1.EquipmentKind != eq2.EquipmentKind)
          previewResult = ReturnCraftingError("Equipment for crafting must be of the same type");

        else if (eq1.WasCraftedBy(RecipeKind.TwoEq) || eq2.WasCraftedBy(RecipeKind.TwoEq))
          previewResult = ReturnCraftingError("Can not craft equipment which was already crafted in pairs");
      }
      else if (lootToConvert.Any(i => i is KeyHalf))
      {
        var parts = Filter<KeyHalf>(lootToConvert);
        if (parts.Count != 2 || !parts[0].Matches(parts[1]))
        {
          previewResult = ReturnCraftingError("Two matching parts of a key are needed");
        }
        //return ReturnCraftedLoot(new Key() { KeyName = (lootToConvert[0] as KeyHalf).KeyName, Kind = KeyKind.BossRoom }); ; ;
      }

      //else if (lootToConvert.Any(i => i is KeyMold))
      //{
      //  //return ConvertMold(recipe, lootToConvert);
      //}
      else
        previewResult = ReturnCraftingError(InvalidIngredients);
      
      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {
      var previewResult = ReturnCraftingError(InvalidIngredients);
      if (eqs.Count == 2)
        previewResult = CraftTwoEq(this.eqs);
      //  return CraftTwoEq(eqs);
      else if (lootToConvert.Any(i => i is KeyHalf))
      {
        previewResult = ReturnCraftedLoot(new Key() { KeyName = (lootToConvert[0] as KeyHalf).KeyName, Kind = KeyKind.BossRoom });
      }

      return previewResult;
    }

    private CraftingResult CraftTwoEq(List<Equipment> eqs)
    {
      var eq1 = eqs[0];
      var eq2 = eqs[1];

      var destEq = eq1.Price > eq2.Price ? eq1 : eq2;

      var srcEq = destEq == eq1 ? eq2 : eq1;
      var srcHadEmptyEnch = srcEq.Enchantable && !srcEq.MaxEnchantsReached();
      var destHadEmptyEnch = destEq.Enchantable && !destEq.MaxEnchantsReached();

      float priceInc = 0;
      var enhPr = GetEnhStatValue(destEq.PrimaryStatValue, destEq.Price, srcEq.Price);
      destEq.PrimaryStatValue += enhPr;
      priceInc += destEq.GetPriceForFactor(destEq.PrimaryStatKind, enhPr);

      var destStats = destEq.GetMagicStats();
      var srcStats = srcEq.GetMagicStats();
      var srcDiffStats1 = srcStats.Where(i => !destStats.Any(j => j.Key == i.Key)).ToList();

      if (destStats.Count < 3 && srcDiffStats1.Any())
      {
        var countToAdd = 3 - destStats.Count;
        foreach (var statToAdd in srcDiffStats1)
        {
          if (destEq.Class == EquipmentClass.Plain)
            destEq.Class = EquipmentClass.Magic;
          destEq.SetMagicStat(statToAdd.Key, statToAdd.Value);
          countToAdd--;
          priceInc += destEq.GetPriceForFactor(statToAdd.Key, (int)statToAdd.Value.Factor);
          if (countToAdd == 0)
            break;
        }
      }
      else
      {
        foreach (var destStat in destStats)
        {
          var enh = GetEnhStatValue(destStat.Value.Factor, destEq.Price, srcEq.Price);
          destStat.Value.Factor += enh;
          priceInc += destEq.GetPriceForFactor(destStat.Key, (int)enh);
          destEq.SetMagicStat(destStat.Key, destStat.Value);
        }
      }

      if (srcHadEmptyEnch || destHadEmptyEnch)
      {
        if (destEq.GetMagicStats().Count < 3 && !destEq.Enchantable)
          destEq.MakeEnchantable();
      }
      destEq.WasCrafted = true;
      destEq.CraftingRecipe = RecipeKind.TwoEq;

      //I noticed price is too high comparing to unique items, maybe price should be calculated from scratch ?
      //priceInc /= 2;

      destEq.Price += (int)priceInc;

      return ReturnCraftedLoot(destEq, eqs.Cast<Loot>().ToList());
    }
  }
}
