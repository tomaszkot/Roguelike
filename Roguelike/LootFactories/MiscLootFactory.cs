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

      InitRepices();

      factory["magic_dust"] = (string tag) =>
      {
        return new MagicDust();
      };

      factory["cord"] = (string tag) =>
      {
        return new Cord();
      };
    }

    private void InitRepices()
    {
      //string[] names;
      Func<RecipeKind, Recipe> createRecipeFromKind = (RecipeKind kind) =>
      {
        var loot = new Recipe();
        loot.Kind = kind;

        return loot;
      };

      var kinds = Enum.GetValues(typeof(RecipeKind)).Cast<RecipeKind>().Where(i => i != RecipeKind.Unset).ToList();
      foreach (var kind in kinds)
      {
        recipesPrototypes.Add(createRecipeFromKind(kind));
      }

      Func<string, Loot> createRecipe = (string tag) =>
      {
        var proto = recipesPrototypes.Where(i => i.tag1 == tag).Single();
        return proto.Clone();
      };

      //var recipeType2Name = new Dictionary<RecipeKind, string>
      //{
      //  { RecipeKind.Custom, "unknown"},
      //  { RecipeKind.ExplosiveCocktail, "expl_cocktail"},
      //  { RecipeKind.Gem, "gem"},
      //  { RecipeKind.OneEq, "craft_one_eq"},
      //  { RecipeKind.Pendant, "craft_pendant"},
      //  { RecipeKind.Potion, "transform_potion"},
      //  { RecipeKind.ThreeGems, "raft_three_gems"},
      //};
      //names = new[] { "craft_one_eq" };
      foreach (var proto in recipesPrototypes)
      {
        factory[proto.tag1] = createRecipe;
      }

      //return names;
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
