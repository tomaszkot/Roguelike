using Dungeons.Core;
using Roguelike.Extensions;
using Roguelike.Tiles;
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
    protected Dictionary<string, Func<string, Loot>> factory = new Dictionary<string, Func<string, Loot>>();
    protected Dictionary<string, Func<string, FightItem>> factoryFightItem = new Dictionary<string, Func<string, FightItem>>();
    
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
        var specialPotionSize = SpecialPotionSize.Small;

        if (tag == "health_potion")
          kind = PotionKind.Health;

        else if (tag == "mana_potion")
          kind = PotionKind.Mana;

        else if (tag == "antidote_potion")
          kind = PotionKind.Antidote;

        else if (tag.Contains("magic_potion"))
        {
          kind = PotionKind.Special;
          specialKind = SpecialPotionKind.Magic;
        }
        else if (tag.Contains("virility_potion"))
        {
          kind = PotionKind.Special;
          specialKind = SpecialPotionKind.Virility;
        }
        else if (tag.Contains("strength_potion"))
        {
          kind = PotionKind.Special;
          specialKind = SpecialPotionKind.Strength;
        }
        else
          Dungeons.DebugHelper.Assert(false);

        if (kind == PotionKind.Special)
        {
          specialPotionSize = SpecialPotionSize.Small;
          if (tag.Contains("big"))
            specialPotionSize = SpecialPotionSize.Big;
          //else if (tag.Contains("medium"))
          //  specialPotionSize = SpecialPotionSize.Medium;
        }

        Potion loot = null;
        if (specialKind != SpecialPotionKind.Unset)
          loot = new SpecialPotion(specialKind, specialPotionSize);
        else
          loot = new Potion(kind);
        return loot;
      };
      var names = new[] { 
        "antidote_potion", 
        "health_potion", 
        "mana_potion", 
        "small_magic_potion", "medium_magic_potion", "big_magic_potion",
        "small_strength_potion" , "medium_strength_potion" ,"big_strength_potion" ,
        "small_virility_potion" ,"medium_virility_potion","big_virility_potion"
      };
      foreach (var name in names)
      {
        factory[name] = createPotion;
      }

      InitRepices();

      factory["magic_dust"] = (string tag) =>
      {
        var md = new MagicDust();
        //md.Count = 100;
        return md;
      };

      factory["hooch"] = (string tag) =>
      {
        return new Hooch();
      };

      factory["feather"] = (string tag) =>
      {
        return new Feather();
      };

      factory["hazel"] = (string tag) =>
      {
        return new Hazel();
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

      factory["key_part_lower"] = (string tag) =>
      {
        var key = new KeyHalf();
        
        return key;
      };

      factory["key_part_upper"] = (string tag) =>
      {
        var key = new KeyHalf();
        key.SetHandlePart();
        return key;
      };

      factory["key_mold"] = (string tag) =>
      {
        var key = new KeyMold();
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

      factory["BarleySack"] = (string tag) =>
      {
        return new GenericLoot("Sack of barley", "Sack full of a dried barley", "BarleySack");
      };

      //PoisonCocktail and others 
      var fis = GetEnumValues<FightItemKind>();
      foreach(var fik in fis)
      {
        Func<string, ProjectileFightItem> cr = (string tag) =>
        {
          var fi = CreateFightItem(fik);
          return fi;
        };

        factory[fik.ToString()] = cr;

        factoryFightItem[fik.ToString()] = cr;
      }

      var tinyTrophies = HunterTrophy.TinyTrophiesTags;
      foreach (var tt in tinyTrophies)
      {
        var kind = HunterTrophyKind.Unset;
        if (tt.EndsWith("claw"))
          kind = HunterTrophyKind.Claw;
        else if (tt.EndsWith("fang"))
          kind = HunterTrophyKind.Fang;
        //else if (tt.EndsWith("tusk"))
        //  kind = HunterTrophyKind.Tusk;

        EnchanterSize enchanterSize = EnchanterSize.Small;
        if (tt.StartsWith("big"))
          enchanterSize = EnchanterSize.Big;
        else if (tt.StartsWith("medium"))
          enchanterSize = EnchanterSize.Medium;

        factory[tt] = (string tag) =>
        {
          return new HunterTrophy(kind) { EnchanterSize = enchanterSize };
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

      factory["cannon"] = (string tag)=> new Cannon();

      //var sli = CreateSanderusLoot();
      //foreach (var sl in sli)
      //  factory[sl.tag1] = (string tag)=> sl;
    }

    public static List<Loot> CreateSanderusLoot()
    {
      var res = new List<Loot>();
      var tags = new[] { "ArchangelGabrielFeathers", "PegansOil", "HoovesOfTheDonkey" , "JacobRungs" };
      foreach (var tag in tags)
      {
        string description = "";
        string name = "";
        if (tag == "ArchangelGabrielFeathers")
        {
          description = "Feathers from the wings of Archangel Gabriel lost during the Annunciation";
          name = "Archangel Gabriel's feathers";
        }
        else if (tag == "PegansOil")
        {
          description = "Oil in which the pagans wanted to fry John the Baptist";
          name = "Saint Oil";
        }
        else if (tag == "HoovesOfTheDonkey")
        {
          description = "Hooves of the donkey on which the Holy Family fled to Egypt";
          name = "Holy Family's Donkey hooves";
        }
        else if (tag == "JacobRungs")
        {
          description = "Rungs from the ladder that patriarch Jacob dreamed of";
          name = "Patriarch Jacob's Rungs";
        }
        var loot = new GenericLoot(name, description, tag);
        loot.Name = name;
        loot.Price = 200;
        
        res.Add(loot);
      }
      return res;
    }

    private static ProjectileFightItem CreateFightItem(FightItemKind fik)
    {
      int max = 3 + RandHelper.GetRandomInt(2);
      int add = 3;
      int min = 1;
      var fi = new ProjectileFightItem(fik, null) { };
      if (fik.IsBowLikeAmmunition())
      {
        max = 20;
        min = 10;
      }
      if (fik == FightItemKind.HunterTrap)
      {
        max = 1;
        add = 2;
      }

      var co = RandHelper.GetRandomInt(max) + add;
      if (co < min)
        co = min;
      fi.Count = co;
      return fi;
    }

    private void InitRepices()
    {
      Func<RecipeKind, Recipe> createRecipeFromKind = (RecipeKind kind) =>
      {
        var loot = new Recipe();
        loot.Kind = kind;

        return loot;
      };

      var kinds = GetEnumValues<RecipeKind>();
      foreach (var kind in kinds)
      {
        if (kind == RecipeKind.Pendant)
          continue;
        recipesPrototypes.Add(createRecipeFromKind(kind));
      }

      Func<string, Loot> createRecipe = (string tag) =>
      {
        var proto = recipesPrototypes.Where(i => i.tag1 == tag).Single();
        return proto.Clone(1);
      };

      foreach (var proto in recipesPrototypes)
      {
        factory[proto.tag1] = createRecipe;
      }
    }

    private static List<T> GetEnumValues<T>() where T : IConvertible
    {
      return EnumHelper.GetEnumValues<T>(true);
    }

    //public override Loot GetByName(string name)
    //{
    //  return GetByAsset(name);
    //}

    public override Loot GetByAsset(string tagPart)
    {
      return GetByAsset(factory, tagPart);
    }

    public override Loot GetRandom(int level)//TODO ! level
    {
      return GetRandom<Loot>(factory);
    }

    public FightItem GetRandomFightItem(int level)//TODO ! level
    {
      var fi = GetRandom<FightItem>(factoryFightItem); 
      while(fi.FightItemKind == FightItemKind.Smoke)
        fi = GetRandom<FightItem>(factoryFightItem);
      return fi;
    }

    public Recipe GetRandomRecipe(int level)//TODO ! level
    {
      return RandHelper.GetRandomElem<Recipe>(recipesPrototypes).Clone(1) as Recipe;
    }
  }
}
