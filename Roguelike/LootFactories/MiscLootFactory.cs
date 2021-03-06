﻿using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        var kind = PotionKind.Unset;
        var specialKind = SpecialPotionKind.Unset;

        if (tag == "health_potion")
          kind = PotionKind.Health;

        else if (tag == "mana_potion")
          kind = PotionKind.Mana;

        else if (tag == "poison_potion")
          kind = PotionKind.Poison;

        else if (tag == "magic_potion")
        {
          kind = PotionKind.Special;
          specialKind = SpecialPotionKind.Magic;
        }

        else if (tag == "strength_potion")
        {
          kind = PotionKind.Special;
          specialKind = SpecialPotionKind.Strength;
        }
        else
          Debug.Assert(false);

        Potion loot = null;
        if (specialKind != SpecialPotionKind.Unset)
          loot = new SpecialPotion(specialKind, SpecialPotionSize.Small);
        else
          loot = new Potion(kind);
        return loot;
      };
      var names = new[] { "poison_potion", "health_potion", "mana_potion", "magic_potion", "strength_potion" };
      foreach (var name in names)
      {
        factory[name] = createPotion;
      }

      InitRepices();

      factory["magic_dust"] = (string tag) =>
      {
        return new MagicDust();
      };

      factory["hour_glass"] = (string tag) =>
      {
        //new GenericLoot("Pick", "Tool for mining", "pick");
        return new GenericLoot("Hourglass", "Hourglass - quite useless tool", "hour_glass");
      };

      factory["gold_chest_key"] = (string tag) =>
      {
        var key = new Key();
        key.Kind = KeyKind.Chest;
        return key;
      };

      factory["cord"] = (string tag) =>
      {
        return new Cord();
      };

      factory["pendant"] = (string tag) =>
      {
        var jew = new Jewellery() { EquipmentKind = EquipmentKind.Amulet };
        jew.SetIsPendant(true);
        return jew;
      };

      factory["goblet"] = (string tag) =>
      {
        return new Goblet() { };
      };

      factory["pick"] = (string tag) =>
      {
        return new GenericLoot("Pick", "Tool for mining", "pick");
      };

      factory["skull"] = (string tag) =>
      {
        return new GenericLoot("Skull of a giant", "Ancient skull of a gaint, worth a couple of coins", "skull");
      };

      factory["coin"] = (string tag) =>
      {
        return new Gold();
      };

      var tinyTrophies = HunterTrophy.TinyTrophiesTags;
      foreach (var tt in tinyTrophies)
      {
        var kind = HunterTrophyKind.Unset;
        if (tt.EndsWith("claw"))
          kind = HunterTrophyKind.Claw;
        else if (tt.EndsWith("fang"))
          kind = HunterTrophyKind.Fang;
        else if (tt.EndsWith("tusk"))
          kind = HunterTrophyKind.Tusk;

        EnchanterSize enchanterSize = EnchanterSize.Small;
        if (tt.StartsWith("big"))
          enchanterSize = EnchanterSize.Big;
        else if (tt.StartsWith("medium"))
          enchanterSize = EnchanterSize.Medium;

        factory[tt] = (string tag) =>
        {
          return new HunterTrophy(kind) { EnchanterSize = enchanterSize, tag1 = tag };
        };
      }

      //gems
      var gemTagTypes = new[] { "diamond", "emerald", "ruby", "amber" };
      var gemTagSizes = new[] { "big", "medium", "small" };
      List<string> gemTags = new List<string>();
      foreach (var gt in gemTagTypes)
      {
        foreach (var gs in gemTagSizes)
        {
          gemTags.Add(gt + "_" + gs);
        }
      }

      foreach (var gemTag in gemTags)
      {
        EnchanterSize enchanterSize = EnchanterSize.Small;
        if (gemTag.EndsWith("big"))
          enchanterSize = EnchanterSize.Big;
        else if (gemTag.EndsWith("medium"))
          enchanterSize = EnchanterSize.Medium;

        GemKind gemKind = GemKind.Diamond;
        if (gemTag.StartsWith("emerald"))
          gemKind = GemKind.Emerald;
        else if (gemTag.StartsWith("ruby"))
          gemKind = GemKind.Ruby;
        else if (gemTag.StartsWith("amber"))
          gemKind = GemKind.Amber;

        factory[gemTag] = (string tag) =>
        {
          return new Gem(gemKind) { EnchanterSize = enchanterSize };
        };
      }
    }

    private void InitRepices()
    {
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

      foreach (var proto in recipesPrototypes)
      {
        factory[proto.tag1] = createRecipe;
      }
    }

    public override Loot GetByName(string name)
    {
      return GetByAsset(name);
    }

    public override Loot GetByAsset(string tagPart)
    {
      var tile = factory.FirstOrDefault(i => i.Key == tagPart);
      if (tile.Key != null)
        return tile.Value(tagPart);

      return null;
    }

    public override Loot GetRandom(int level)//TODO level
    {
      return GetRandom<Loot>(factory);
    }
  }
}
