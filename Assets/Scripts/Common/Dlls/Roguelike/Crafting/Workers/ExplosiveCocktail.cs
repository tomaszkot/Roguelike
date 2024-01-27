using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System.Collections.Generic;

namespace Roguelike.Crafting.Workers
{
  public class ExplosiveCocktail : CraftWorker
  {
    //StackedLoot sulfur;
    //Hooch hooch;

    public ExplosiveCocktail()
    {
      Kind = RecipeKind.ExplosiveCocktail;
    }
    public ExplosiveCocktail(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.ExplosiveCocktail;
    }

    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = lootCrafter.GetMinedLootType() };//Sulfur
     // var i2 = new RecipeRequiredItem() { Type = typeof(MagicDust), MinCount = 1 };
      var i3 = new RecipeRequiredItem() { Type = typeof(Hooch) };
      ri.Items.Add(i1);
      //ri.Items.Add(i2);
      ri.Items.Add(i3);
      return ri;
    }


    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;

      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {
      return null;
    }
  }
}
