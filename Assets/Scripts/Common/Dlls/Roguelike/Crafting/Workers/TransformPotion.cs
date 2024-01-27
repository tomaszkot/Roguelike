using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Extensions;

namespace Roguelike.Core.Crafting.Workers
{
  internal class TransformPotion : CraftWorker
  {
    //Potion hps;
    Potion srcPotion;
    Mushroom srcToad;
    internal TransformPotion(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.TransformPotion;
    }
    
    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = typeof(Potion) };
      //var i2 = new RecipeRequiredItem() { Type = typeof(MagicDust), MinCount = 1 };
      var i3 = new RecipeRequiredItem() { Type = typeof(Mushroom) };

      ri.Items.Add(i1);
      //ri.Items.Add(i2);
      ri.Items.Add(i3);
      return ri;
    }

    bool CanCraftHP()
    {
      srcPotion = stacked.Where(i => i.IsPotionKind(PotionKind.Mana)).Cast<Potion>().FirstOrDefault();
      if (srcPotion == null)
        return false;

      srcToad = lootToConvert.Where(i => i.IsToadstool()).Cast<Mushroom>().Where(i => i.MushroomKind == MushroomKind.RedToadstool).FirstOrDefault();
      if (srcToad == null)
        return false;
      return true;
    }

    bool CanCraftMP()
    {
      srcPotion = stacked.Where(i => i.IsPotionKind(PotionKind.Health)).Cast<Potion>().FirstOrDefault();
      if (srcPotion == null)
        return false;

      srcToad = lootToConvert.Where(i => i.IsToadstool()).Cast<Mushroom>().Where(i=> i.MushroomKind == MushroomKind.BlueToadstool).FirstOrDefault();
      if (srcToad == null)
        return false;
      return true;
    }

    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;
      if(!CanCraftHP() && !CanCraftMP())
        return ReturnCraftingError(InvalidIngredients);

      // new Loot[] { potion, toad };

      //hps = stacked.Where(i => i.IsPotionKind(PotionKind.Health)).Cast<Potion>().FirstOrDefault();
      //mps = stacked.Where(i => i.IsPotionKind(PotionKind.Mana)).Cast<Potion>().FirstOrDefault();
      //if (hps == null && mps == null)
      //  previewResult = ReturnCraftingError("At least one Mana or Health potion is required");

      //if (hps != null && mps != null)
      //  return ReturnCraftingError("Source potions must be of one kind");

      //var toadstools = lootToConvert.Where(i => i.IsToadstool()).Cast<Mushroom>().ToList();
      //if (!toadstools.Any())
      //{
      //  return ReturnCraftingError("Toadstool is required");
      //}

      //var ts = toadstools.GroupBy(i => i.MushroomKind);
      ////if (ts.Count() > 1)
      ////{
      ////  return ReturnCraftingError("Toadstools must be of one kind");
      ////}
      //if (hps != null && mush.MushroomKind == MushroomKind.RedToadstool ||
      //   mps !=null  && mush.MushroomKind == MushroomKind.BlueToadstool)
      //return ReturnCraftingError("Invalid toadstool kind provided");
      //toad = mush;
      previewResult = new CraftingResult(lootToConvert) { UsedInputItems = new List<Loot>{ srcPotion, srcToad } };
      return ReturnCanDo(previewResult);
            
    }

    public override CraftingResult Do()
    {
      var toadstoolsCount = srcToad.Count;

      var inputLoot = new Loot[] { srcPotion, srcToad };
      var ct = GetCraftedStackedCount(inputLoot);
      if (srcPotion.AsPotion().Kind == PotionKind.Mana)
      {        
        if (srcToad.MushroomKind == MushroomKind.RedToadstool)
          return ReturnCraftedLoot(new Potion(PotionKind.Health) { Count = ct }, inputLoot.ToList());
      }
      else
      {
        if (srcToad.MushroomKind == MushroomKind.BlueToadstool)
          return ReturnCraftedLoot(new Potion(PotionKind.Mana) { Count = ct }, inputLoot.ToList());
      }
      return ReturnCraftingError(InvalidIngredients);
    }
  }
}
