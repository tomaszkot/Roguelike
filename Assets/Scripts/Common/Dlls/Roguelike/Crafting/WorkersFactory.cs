using Dungeons.Core;
using Roguelike.Core.Crafting.Workers;
using Roguelike.Crafting.Workers;
using Roguelike.Factors;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Crafting
{
  internal class WorkersFactory
  {
    static List<RecipeKind> recipeKinds;
    Dictionary<RecipeKind, CraftWorker> factory = new Dictionary<RecipeKind, CraftWorker>();
    List<Loot> lootToConvert;
   
    static WorkersFactory()
    {
      recipeKinds = EnumHelper.GetEnumValues<RecipeKind>(true);
      recipeKinds.Remove(RecipeKind.Custom);
    }

    public WorkersFactory(List<Loot> lootToConvert, ILootCrafter lootCrafter, Container container)
    {
      this.lootToConvert = lootToConvert;
      factory[RecipeKind.EnchantEquipment] = new EnchantEquipment(lootToConvert, lootCrafter);
      factory[RecipeKind.TwoEq] = new TwoEquipments(lootToConvert, lootCrafter);
      factory[RecipeKind.UnEnchantEquipment] = new UnEnchantEquipment(lootToConvert, lootCrafter);
      factory[RecipeKind.OneEq] = new OneEq(lootToConvert, lootCrafter);
      factory[RecipeKind.CraftSpecialPotion] = new Core.Crafting.Workers.SpecialPotion(lootToConvert, lootCrafter);
      factory[RecipeKind.Arrows] = container.GetInstance<Roguelike.Crafting.Workers.Arrows>();
      factory[RecipeKind.Arrows].Init(lootToConvert, lootCrafter);
      factory[RecipeKind.Bolts] = container.GetInstance<Roguelike.Crafting.Workers.Bolts>();
      factory[RecipeKind.Bolts].Init(lootToConvert, lootCrafter);
      factory[RecipeKind.NiesiolowskiSoup] = new NiesiolowskiSoup(lootToConvert, lootCrafter);
      factory[RecipeKind.ThreeGems] = new ThreeGems(lootToConvert, lootCrafter);
      factory[RecipeKind.TransformPotion] = new TransformPotion(lootToConvert, lootCrafter);
      factory[RecipeKind.TransformGem] = new TransformGem(lootToConvert, lootCrafter);
      factory[RecipeKind.Toadstools2Potion] = new Toadstools2Potion(lootToConvert, lootCrafter);
      factory[RecipeKind.ExplosiveCocktail] = container.GetInstance <Roguelike.Crafting.Workers.ExplosiveCocktail>();
      factory[RecipeKind.ExplosiveCocktail].Init(lootToConvert, lootCrafter);

      factory[RecipeKind.RechargeMagicalWeapon] = new RechargeMagicalWeapon(lootToConvert, lootCrafter);
      factory[RecipeKind.AntidotePotion] = new AntidotePotion(lootToConvert, lootCrafter);
    }

    void EnsureWorker(RecipeKind kind)
    { 
    
    }

    internal CraftWorker GetWorker(RecipeKind kind)
    {
      if (kind == RecipeKind.Custom)
      {
        List<CraftWorker> matches = new List<CraftWorker>();
        recipeKinds.ForEach(i => {
          var workerReal = GetWorker(i);
          if (workerReal != null)
          {
            if(workerReal.CanDo().Success)
              matches.Add(workerReal);
          }
        });
        var ench = matches.FirstOrDefault(i => i.Kind == RecipeKind.EnchantEquipment);
        if (ench != null)
          return ench;

        ench = matches.FirstOrDefault(i => i.Kind == RecipeKind.TwoEq);
        if (ench != null)
          return ench;

        return matches.GetRandomElem();

      }
      var worker = factory.ContainsKey(kind) ? factory[kind] : null;
      if (worker != null)
      {
        return worker;
      }
      return null;
    }
  }
}
