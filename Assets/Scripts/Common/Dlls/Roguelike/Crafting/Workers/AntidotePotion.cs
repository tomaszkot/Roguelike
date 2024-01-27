using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Core.Crafting.Workers
{
  internal class AntidotePotion : CraftWorker
  {
    Plant plant;
    Hooch hooch;
    internal AntidotePotion(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.AntidotePotion;
    }

    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = typeof(Plant) };
      //var i2 = new RecipeRequiredItem() { Type = typeof(MagicDust), MinCount = 1 };
      var i3 = new RecipeRequiredItem() { Type = typeof(Hooch) };
      ri.Items.Add(i1);
      //ri.Items.Add(i2);
      ri.Items.Add(i3);
      return ri;
    }

    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;
      plant = Filter<Plant>(lootToConvert).Where(i => i.Kind == PlantKind.Thistle).SingleOrDefault();
      if (plant ==null)
        return ReturnCraftingError("Thistle is required by the Recipe");

      hooch = FilterOne<Hooch>(lootToConvert); 
      if (hooch == null)
        return ReturnCraftingError("Hooch is required  by the Recipe");
      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {
      var ct = GetCraftedStackedCount(new [] { plant.Count, hooch.Count });
      return ReturnCraftedLoot(new Potion(PotionKind.Antidote) { Count = ct }, new List<Loot>() { plant, hooch },  true);
 
    }
  }
}
