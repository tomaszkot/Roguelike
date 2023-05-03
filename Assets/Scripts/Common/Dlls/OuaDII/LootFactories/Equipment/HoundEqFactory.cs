using Dungeons.Core;
using OuaDII.Tiles.Looting.Equipment;
using Roguelike.Attributes;
using Roguelike.Generators;
using Roguelike.LootFactories;
using Roguelike.Probability;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.LootFactories.Equipment
{
  class HoundEqFactory : EquipmentTypeFactory
  {
    public HoundEqFactory(Container cont) : base(cont)
    {
    }

    public Roguelike.Tiles.Loot GetRandom(Roguelike.Tiles.EquipmentKind kind, int level)
    {
      var all = GetAll().Cast<Roguelike.Tiles.Looting.Equipment >().Where(i => i.EquipmentKind == kind).ToList();
      var mat = Roguelike.Tiles.Looting.EquipmentMaterial.Unset;
      var lootDesc = GetRandomFromList(level, mat, all);

      var loot = CreateItem(level, lootDesc);

      return loot;
    }

    protected override void Create()
    {
      CreateArmours();
      //CreateShields();
      CreateHelmets();
      CreateGloves();
      CreateWeapons();

      CreatePrototypes();
    }

    void CreateWeapons()
    {
      Func<string, Roguelike.Tiles.Looting.Equipment> createArm = (string asset) =>
      {
        var wpn = new Weapon();
        wpn.Kind = Roguelike.Tiles.Looting.Weapon.WeaponKind.Other;
        wpn.MatchingAnimalKind = Roguelike.Tiles.LivingEntities.AnimalKind.Hound;
        wpn.tag1 = asset;
        SetNameFromAsset(asset, wpn);

        if (asset == "hound_boned_jaws")
        {
          wpn.Damage = 2;
          wpn.SetLevelIndex(4);
        }
        else if (asset == "hound_bronze_jaws")
        {
          wpn.Damage = 4;
          wpn.SetLevelIndex(6);
        }
        else if (asset == "hound_iron_jaws")
        {
          wpn.Damage = 7;
          wpn.SetLevelIndex(8);
        }
        else if (asset == "hound_steel_jaws")
        {
          wpn.Damage = 10;
          wpn.SetLevelIndex(10);
        }

        return wpn;
      };
      var names = new[] { "hound_boned_jaws", "hound_bronze_jaws", "hound_iron_jaws", "hound_steel_jaws" };
      foreach (var name in names)
      {
        factory[name] = createArm;
      }
    }

    void CreateArmours()
    {
      Func<string, Roguelike.Tiles.Looting.Equipment> createArm = (string asset) =>
      {
        var arm = new Roguelike.Tiles.Looting.Armor();
        arm.MatchingAnimalKind = Roguelike.Tiles.LivingEntities.AnimalKind.Hound;
        arm.tag1 = asset;
        SetNameFromAsset(asset, arm);

        if (asset == "hound_tunic")
        {
          arm.Defense = 2;
          arm.SetLevelIndex(2);
        }
        else if (asset == "solid_hound_tunic")
        {
          arm.Defense = 5;
          arm.SetLevelIndex(4);
        }
        else if (asset == "leather_hound_coat")
        {
          arm.Defense = 8;
          arm.SetLevelIndex(7);
        }
        else if (asset == "bronze_hound_armor")
        {
          arm.Defense = 12;
          arm.SetLevelIndex(7);
          arm.SetMaterial(Roguelike.Tiles.Looting.EquipmentMaterial.Bronze);
        }
        else if (asset == "iron_hound_armor")
        {
          arm.Defense = 15;
          arm.SetLevelIndex(10);
          arm.SetMaterial(Roguelike.Tiles.Looting.EquipmentMaterial.Iron);
        }
        else if (asset == "steel_hound_armor")
        {
          arm.Defense = 20;
          arm.SetLevelIndex(12);
          arm.SetMaterial(Roguelike.Tiles.Looting.EquipmentMaterial.Steel);
        }
       
        return arm;
      };
      var names = new[] { "hound_tunic", "solid_hound_tunic", "leather_hound_coat", "bronze_hound_armor", "iron_hound_armor", "steel_hound_armor" };
      foreach (var name in names)
      {
        factory[name] = createArm;
      }
    }

    void CreateGloves()
    {
      Func<string, Roguelike.Tiles.Looting.Equipment > createArm = (string asset) =>
      {
        var eq = new Armor();
        eq.EquipmentKind = Roguelike.Tiles.EquipmentKind.Glove;
        eq.MatchingAnimalKind = Roguelike.Tiles.LivingEntities.AnimalKind.Hound;
        eq.tag1 = asset;
        SetNameFromAsset(asset, eq);

        if (asset == "hound_leather_paws")
        {
          eq.Defense = 2;
          eq.SetLevelIndex(1);
        }
        else if (asset == "hound_bronze_paws")
        {
          eq.Defense = 4;
          eq.SetLevelIndex(6);
        }
        else if (asset == "hound_iron_paws")
        {
          eq.Defense = 6;
          eq.SetLevelIndex(8);
        }
        else if (asset == "hound_steel_paws")
        {
          eq.Defense = 8;
          eq.SetLevelIndex(10);
        }

        eq.SetLevelIndex(1);

        return eq;
      };
      var names = new[] { "hound_leather_paws", "hound_bronze_paws", "hound_iron_paws", "hound_steel_paws" };
      foreach (var name in names)
      {
        factory[name] = createArm;
      }
    }

    private static Roguelike.Tiles.Looting.Armor CreateHelmet(string asset, int defense)
    {
      var arm = new Roguelike.Tiles.Looting.Armor();
      arm.tag1 = asset;
      arm.EquipmentKind = Roguelike.Tiles.EquipmentKind.Helmet;
      arm.MatchingAnimalKind = Roguelike.Tiles.LivingEntities.AnimalKind.Hound;
      SetNameFromAsset(asset, arm);
      return arm;
    }

    private static void SetNameFromAsset(string asset, Roguelike.Tiles.Looting.Equipment eq)
    {
      var name = asset.ToUpperFirstLetter();
      name = name.Replace("_", " ");
      eq.Name = name;
    }

    void CreateHelmets()
    {
      //Func<string, Armor> createArm = (string asset) =>
      //{
      //  var arm = new Armor();
      //  arm.EquipmentKind = Roguelike.Tiles.Looting.EquipmentKind.Helmet;
      //  arm.Defense = 5;
      //  arm.tag1 = asset;
      //  arm.SetLevelIndex(8);

      //  var ees = new EqEntityStats();
      //  ees.Add(EntityStatKind.Health, 15)
      //  .Add(EntityStatKind.MeleeAttack, 15)
      //  .Add(EntityStatKind.ChanceToMeleeHit, 5);

      //  arm.SetUnique(ees.Get(), 5);

      //  return arm;
      //};
      //factory["war_helm"] = createArm;


      Func<string, Roguelike.Tiles.Looting.Equipment> createHelm = (string asset) =>
      {
        var arm = CreateHelmet(asset, 2);
        int level = 1;
               
        if (asset == "hound_cap")
        {
          arm.Defense = 2;//3
          
          level = 1;//3
        }
        else if (asset == "solid_hound_cap")
        {
          arm.Defense = 4;
          level = 5;
        }
        else if (asset == "bronze_hound_cap")
        {
          arm.Defense = 7;
          arm.SetMaterial(Roguelike.Tiles.Looting.EquipmentMaterial.Bronze);
          level = 7;
        }
        else if (asset == "iron_hound_cap")
        {
          arm.Defense = 10;
          arm.SetMaterial(Roguelike.Tiles.Looting.EquipmentMaterial.Iron);
          level = 9;
        }
        else if (asset == "steel_hound_cap")
        {
          arm.Defense = 14;
          arm.SetMaterial(Roguelike.Tiles.Looting.EquipmentMaterial.Steel);
          level = 12;
        }

        arm.SetLevelIndex(level);

        return arm;
      };
      var helms = new[] { "hound_cap", "solid_hound_cap", "bronze_hound_cap", "iron_hound_cap",  "steel_hound_cap"};
      foreach (var helm in helms)
      {
        factory[helm] = createHelm;
      }
    }
  }
}
