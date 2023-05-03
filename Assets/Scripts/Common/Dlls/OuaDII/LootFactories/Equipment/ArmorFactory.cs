using Dungeons.Core;
using OuaDII.Tiles.Looting.Equipment;
using Roguelike.Attributes;
using Roguelike.Generators;
using Roguelike.LootFactories;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OuaDII.LootFactories.EquipmentFactory;

namespace OuaDII.LootFactories.Equipment
{
  class ArmorFactory : EquipmentTypeFactory
  {
    public ArmorFactory(Container cont) : base(cont)
    {
    }

    protected override void Create()
    {
      CreateArmours();
      CreateShields();
      CreateHelmets();
      CreateGloves();

      CreatePrototypes();
    }



    void CreateShields()
    {
      Func<string, Tiles.Looting.Equipment.Armor> createShield = (string asset) =>
      {
        var arm = new Tiles.Looting.Equipment.Armor();
        arm.EquipmentKind = Roguelike.Tiles.EquipmentKind.Shield;
        arm.tag1 = asset;

        if (asset == "damaged_buckler")
        {
          arm.Defense = 2;
          arm.SetLevelIndex(1);
        }
        else if (asset == "buckler")
        {
          arm.Defense = 3;
          arm.SetLevelIndex(2);
        }
        else if (asset == "edged_buckler")
        {
          arm.Defense = 4;
          arm.SetLevelIndex(3);
        }
        else if (asset == "enhanced_buckler")
        {
          arm.Defense = 5;
          arm.SetLevelIndex(4);
        }
        else if (asset == "long_shield")
        {
          arm.Defense = 6;
          arm.SetLevelIndex(5);
        }
        else if (asset == "war_shield")
        {
          arm.Defense = 7;
          arm.SetLevelIndex(6);
        }
        else if (asset == "king's_bucket")
        {
          arm.Defense = 8;
          arm.SetLevelIndex(8);
        }

        return arm;
      };

      factory["damaged_buckler"] = createShield;
      factory["buckler"] = createShield;
      factory["edged_buckler"] = createShield;
      factory["enhanced_buckler"] = createShield;
      factory["long_shield"] = createShield;
      factory["war_shield"] = createShield;
      factory["king's_bucket"] = createShield;


    }

    public const int IronedLeatherArmorDefense = 10;

    void CreateArmours()
    {
      Func<string, Tiles.Looting.Equipment.Armor> createArm = (string asset) =>
      {
        var arm = new Armor();
        arm.tag1 = asset;
        int startStr = 10;

        if (asset == "ragged_tunic")
        {
          arm.Defense = 2;
          arm.SetLevelIndex(1);
        }
        //else if (asset == "silk_shirt")
        //{
        //  arm.Defense = 2;
        //  arm.SetLevelIndex(1);
        //}
        else if (asset == "tunic")
        {
          arm.Defense = 5;
          arm.SetLevelIndex(3);
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 2);
        }
        else if (asset == "cape")
        {
          arm.Defense = 8;
          arm.SetLevelIndex(5);
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 4);
        }
        else if (asset == "padded_armor")
        {
          arm.Defense = 12;
          arm.SetLevelIndex(7);
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 6);
        }
        else if (asset == "leather_armor")
        {
          arm.Defense = 20;
          arm.SetLevelIndex(9);
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 8);
        }
        else if (asset == "solid_leather_armor")
        {
          arm.Defense = 30;
          arm.SetLevelIndex(11);
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 10);
        }
        else if (asset == "chained_armor")
        {
          arm.Defense = 40;
          arm.SetLevelIndex(13);
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 20);
        }
        else if (asset == "plate_armor")
        {
          arm.Defense = 45;
          arm.SetLevelIndex(15);
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 40);
        }
        else if (asset == "full_plate_armor")
        {
          arm.Defense = 50;
          arm.SetLevelIndex(17);
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 50);
        }
        //else if (asset == "ironed_leather_armor")
        //{
        //  arm.Defense = IronedLeatherArmorDefense;
        //  arm.SetLevelIndex(5);
        //}

        return arm;
      };
      var names = new[] { "ragged_tunic", /*"silk_shirt",*/ "cape", "tunic", "padded_armor", "leather_armor", "solid_leather_armor",
        "chained_armor", "plate_armor", "full_plate_armor" };
      foreach (var name in names)
      {
        factory[name] = createArm;
      }
    }

    void CreateGloves()
    {
      var names = new[] { "worn_gloves", "leather_gloves", "enhanced_leather_gloves", "chained_gloves", "gauntlets" };
      foreach (var name in names)
      {
        factory[name] = CreateGlove;
      }
    }

    private static Armor CreateGlove(string asset)
    {
      var arm = new Armor();
      arm.tag1 = asset;
      arm.EquipmentKind = Roguelike.Tiles.EquipmentKind.Glove;
      arm.Name = asset.ToUpperFirstLetter();
      arm.Name = arm.Name.Replace("_", " ");

      int level = 1;
      if (asset == "worn_gloves")
      {
        arm.Defense = 1;
      }
      else if (asset == "leather_gloves")
      {
        arm.Defense = 3;
        level = 2;
      }
      else if (asset == "enhanced_leather_gloves")
      {
        arm.Defense = 5;
        level = 3;
      }
      else if (asset == "chained_gloves")
      {
        arm.Defense = 10;
        level = 4;
      }
      else if (asset == "gauntlets")
      {
        arm.Defense = 15;
        level = 5;
      }
      arm.SetLevelIndex(level);
      //arm.SetPrimaryStat(EntityStatKind.Defense, 2);

      return arm;
    }

    private static Armor CreateHelmet(string asset, int defense)
    {
      var arm = new Armor();
      arm.tag1 = asset;
      arm.EquipmentKind = Roguelike.Tiles.EquipmentKind.Helmet;
      var name = asset.ToUpperFirstLetter();
      name = name.Replace("_", " ");
      arm.Name = name;
      return arm;
    }

    void CreateHelmets()
    {
      Func<string, Armor> createArm = (string asset) =>
      {
        var arm = new Armor();
        arm.EquipmentKind = Roguelike.Tiles.EquipmentKind.Helmet;
        arm.Defense = 5;
        arm.tag1 = asset;
        arm.SetLevelIndex(8);

        var ees = new EqEntityStats();
        ees.Add(EntityStatKind.Health, 15)
        .Add(EntityStatKind.MeleeAttack, 15)
        .Add(EntityStatKind.ChanceToMeleeHit, 5);

        arm.SetUnique(ees.Get(), 5);

        return arm;
      };
      factory["war_helm"] = createArm;
      //prototypes.Add("war_helm", createArm("war_helm"));

      int startStr = 10;
      Func<string, Armor> createHelm = (string asset) =>
      {
        var arm = CreateHelmet(asset, 2);
        int level = 1;

        if (asset == "cap")
        {
          arm.Defense = 2;
          level = 1;
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 2);
        }
        else if (asset == "enhanced_cap")
        {
          arm.Defense = 4;
          level = 2;
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 4);
        }
        else if (asset == "helm")
        {
          arm.Defense = 8;
          level = 3;
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 6);
        }
        else if (asset == "full_helm")
        {
          arm.Defense = 12;
          level = 4;
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 10);
        }
        else if (asset == "holly_helm")
        {
          arm.Defense = 16;
          level = 5;
          arm.RequiredStats.SetNominal(EntityStatKind.Strength, startStr + 15);
        }
        
        arm.SetLevelIndex(level);

        return arm;
      };
      var helms = new[] { "cap", "enhanced_cap", "helm", "holly_helm", "full_helm"};
      foreach (var helm in helms)
      {
        factory[helm] = createHelm;
      }
    }
  }
}
