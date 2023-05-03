using Roguelike.Crafting;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Crafting
{
  public class LootCrafter : Roguelike.Crafting.LootCrafter
  {
    public  LootCrafter(Container container) : base(container)
    {
    }
    protected override CraftingResult ConvertBoltsOrArrows(Recipe recipe, List<Loot> lootToConvert)
    {
      var stones = Filter<FightItem>(lootToConvert).Where(i => i.FightItemKind == FightItemKind.Stone).FirstOrDefault();
      var ironOre = Filter<MinedLoot>(lootToConvert).Where(i => i.Kind == MinedLootKind.IronOre).ToList();
      var tip = "a stone or iron ore is required";
      if (stones == null && !ironOre.Any())
        return ReturnCraftingError(tip);

      var feather = Filter<Feather>(lootToConvert).FirstOrDefault();
      if (feather == null)
        return ReturnCraftingError("a feather is required");
      var hazels = Filter<Hazel>(lootToConvert).FirstOrDefault();
      if (hazels == null)
        return ReturnCraftingError("a hazel is required");


      //var ironOreCount = ironOre.Count;
      //var stonesCount = stones.Count;
      var fik = recipe.Kind == RecipeKind.Arrows ? FightItemKind.PlainArrow : FightItemKind.PlainBolt;
      if (ironOre.Any())
      {
        fik = recipe.Kind == RecipeKind.Arrows ? FightItemKind.IronArrow : FightItemKind.IronBolt;
      }
      var eq = new ProjectileFightItem(fik);
     
      eq.Count = GetCraftedStackedCount(lootToConvert);
      return ReturnCraftedLoot(eq);
    }

    protected override CraftingResult ConvertMold(Recipe recipe, List<Loot> lootToConvert)
    {
      var loot = FilterOne<Roguelike.Tiles.Looting.MinedLoot>(lootToConvert);
      var mold = FilterOne<KeyMold>(lootToConvert);

      if (loot !=null && loot.Kind == MinedLootKind.IronOre)
      {
        return ReturnCraftedLoot(new Key() { KeyName = mold.KeyName , Kind = KeyKind.BossRoom });
      }
      return ReturnCraftingError("Iron Ore is required");
    }
  }
}
