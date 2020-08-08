using System;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;

namespace Roguelike.LootFactories
{
  public class MiscLootFactory : AbstractLootFactory
  {
    protected Dictionary<string, Func<string, Loot>> factory =
     new Dictionary<string, Func<string, Loot>>();
    List<Recipe> recipesPrototypes = new List<Recipe>();

    public MiscLootFactory(Container container) : base(container)
    {
    }

    protected override void Create()
    {
      Func<string, Loot> createPotion = (string tag) =>
      {
        var loot = new Potion();
        loot.tag1 = tag;
        if (tag == "poison_potion")
          loot.SetKind(PotionKind.Poison);
        else if (tag == "health_potion")
          loot.SetKind(PotionKind.Health);
        else if (tag == "mana_potion")
          loot.SetKind(PotionKind.Mana);
        return loot;
      };
      var names = new[] { "poison_potion", "health_potion", "mana_potion" };
      foreach (var name in names)
      {
        factory[name] = createPotion;
      }

      Func<RecipeKind, Recipe> createRecipeFromKind = (RecipeKind kind) =>
      {
        var loot = new Recipe();
        loot.Kind = kind;

        return loot;
      };
            
      var kinds = new[] { RecipeKind.OneEq };
      foreach (var kind in kinds)
      {
        recipesPrototypes.Add(createRecipeFromKind(kind));
      }

      Func<string, Loot> createRecipe = (string tag) =>
      {
        var proto = recipesPrototypes.Where(i => i.tag1 == tag).Single();
        return proto.Clone();
      };

      names = new[] { "craft_one_eq" };
      foreach (var name in names)
      {
        factory[name] = createRecipe;
      }

      factory["magic_dust"] = (string tag) =>
      {
        return new MagicDust();
      };
    }

    public override Loot GetByName(string name)
    {
      return GetByTag(name);
    }

    public override Loot GetByTag(string tagPart)
    {
      var tile = factory.FirstOrDefault(i => i.Key == tagPart);
      if (tile.Key != null)
        return tile.Value(tagPart);

      return null;
    }

    public override Loot GetRandom()
    {
      return GetRandom<Loot>(factory);
    }
  }
}
