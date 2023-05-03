using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.LootFactories;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Roguelike.Tiles.Looting.Weapon;

namespace OuaDII.LootFactories.Equipment
{
  class WeaponFactory : EquipmentTypeFactory
  {
    const int MaxLevel = 15;

    public WeaponFactory(Container cont) : base(cont)
    {
    }

    void SetSpellSource(Roguelike.Tiles.Looting.Weapon wpn, string asset)
    {
      if (asset.Contains("fire"))
        wpn.SpellSource.Kind = Roguelike.Spells.SpellKind.FireBall;
      else if (asset.Contains("ice"))
        wpn.SpellSource.Kind = Roguelike.Spells.SpellKind.IceBall;
      else if (asset.Contains("poison"))
        wpn.SpellSource.Kind = Roguelike.Spells.SpellKind.PoisonBall;
    }

    void CreateMagical()
    {
      Func<string, Roguelike.Tiles.Looting.Weapon> createWpn = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.tag1 = asset;
        wpn.SetNameFromTag1();
        wpn.Price *= 5;

        var levelFromAsset = 1;

        if (asset.Contains("scepter"))
        {
          wpn.Kind = Roguelike.Tiles.Looting.Weapon.WeaponKind.Scepter;

          //wpn.SetLevelIndex(2);
        }
        else if (asset.Contains("wand"))
        {
          wpn.Kind = Roguelike.Tiles.Looting.Weapon.WeaponKind.Wand;
        }
        else if (asset.Contains("staff"))
        {
          wpn.Kind = Roguelike.Tiles.Looting.Weapon.WeaponKind.Staff;
        }

        //wpn.SetLevelIndex(1);

        var ind = asset.IndexOf(wpn.Kind.ToString().ToLower()) + wpn.Kind.ToString().Length;
        levelFromAsset = Int32.Parse(asset.Substring(ind, asset.Length - ind));

        SetSpellSource(wpn, asset);
        var wss = (wpn.SpellSource as WeaponSpellSource);
        if (levelFromAsset == 2)
        {
          int k = 0;
          k++;
        }

        wpn.SetLevelIndex(levelFromAsset);
        int imgIndex = 1;
        if (levelFromAsset > 4)
          imgIndex = 2;
        if (levelFromAsset > 8)
          imgIndex = 3;
        wpn.tag1 = asset;
        //asset can not be used as it does not exists
        wpn.tag2 = wss.Kind.ToString().ToLower().Replace("ball", "") + "_" + wpn.Kind.ToString().ToLower()+ 1;
        wpn.tag1 = wpn.tag2;//TODO draw images

        wpn.Name = wss.Kind + " " + wpn.Kind;
        //wpn.Name += " fac!";
        wpn.SetInitChargesCount(imgIndex);
        wpn.UpdateMagicWeaponDesc();
        
        return wpn;
      };

      List<string> names = new List<string>();
      var elems = new[] { "fire", "ice", "poison" };
      var types = new[] { "wand", "scepter", "staff" };
      //var elems = new[] { "fire" };
      //var types = new[] { "scepter" };
      for (int i = 0; i < MaxLevel; i++)
      {
        foreach (var el in elems)
        {
          foreach (var type in types)
          {
            var asset = el + "_" + type + (i + 1);
            names.Add(asset);
          }
        }
      }


      foreach (var name in names)
        factory[name] = createWpn;
    }
     
    void CreateBows()
    {
      Func<string, Weapon> createWpn = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = Weapon.WeaponKind.Bow;
        //var startDmg = Roguelike.LootFactories.Props.BowBaseDamage;

        if (asset == "crude_bow")
        {
          wpn.SetLevelIndex(1);
        }
        else if (asset == "bow_level2")
        {
          wpn.SetLevelIndex(2);
        }
        else if (asset == "bow")
        {
          wpn.SetLevelIndex(3);
        }
        else if (asset == "bow_level4")
        {
          wpn.SetLevelIndex(4);
        }
        else if (asset == "solid_bow")
        {
          wpn.SetLevelIndex(5);
        }
        else if (asset == "solid_bow_level6")
        {
          wpn.SetLevelIndex(6);
        }
        else if (asset == "composite_bow")
        {
          wpn.SetLevelIndex(7);
        }
        else if (asset == "war_bow")
        {
          wpn.SetLevelIndex(11);
        }
        FinishWeaponInit(asset, wpn);
        return wpn;
      };

      var names = new[] { "crude_bow", "bow_level4", "bow_level2", "bow", "solid_bow", "solid_bow_level6", "composite_bow", "war_bow" };
      foreach (var name in names)
        factory[name] = createWpn;
    }

    void CreateCrossbows()
    {
      Func<string, Weapon> createWpn = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        
        wpn.Kind = Weapon.WeaponKind.Crossbow;
        //var startDmg = Props.CrossbowBaseDamage;
        //var dgmStep = 3;

        if (asset == "crude_crossbow")
        {
          wpn.SetLevelIndex(1);
        }
        else if (asset == "crossbow_level2")
        {
          wpn.SetLevelIndex(2);
        }
        else if (asset == "crossbow")
        {
          wpn.SetLevelIndex(3);
        }
        else if (asset == "solid_crossbow")
        {
          wpn.SetLevelIndex(5);
        }
        else if (asset == "composite_crossbow")
        {
          wpn.SetLevelIndex(7);
        }
        else if (asset == "war_crossbow")
        {
          wpn.SetLevelIndex(11);
        }
        FinishWeaponInit(asset, wpn);
        return wpn;
      };

      var names = new[] { "crude_crossbow", "crossbow_level2", "crossbow", "solid_crossbow", "composite_crossbow", "war_crossbow" };
      foreach (var name in names)
        factory[name] = createWpn;
    }

    void CreateAxes()
    {
      Func<string, Weapon> createWpn = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = Weapon.WeaponKind.Axe;

        if (asset == "sickle")
        {
          wpn.SetLevelIndex(1);
        }
        else if (asset == "hatchet")
        {
          wpn.SetLevelIndex(2);
        }
        else if (asset == "solid_hatchet")
        {
          wpn.SetLevelIndex(4);
        }
        else if (asset == "axe")
        {
          wpn.SetLevelIndex(6);
        }
        else if (asset == "solid_axe")
        {
          wpn.SetLevelIndex(8);
        }
        else if (asset == "double_axe")
        {
          wpn.SetLevelIndex(10);
        }
        else if (asset == "war_axe")
        {
          wpn.SetLevelIndex(12);
          //Debug.Assert(wpn.Material == Roguelike.Tiles.EquipmentMaterial.Steel);
        }
        FinishWeaponInit(asset, wpn);
        return wpn;
      };

      var names = new[] { "sickle", "hatchet", "solid_hatchet",  "axe", "solid_axe", "double_axe", "war_axe" };
      foreach (var name in names)
        factory[name] = createWpn;
    }

    void CreateDaggers()
    {
      Func<string, Weapon> createWpn = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Dagger;

        if (asset == "stabber")
        {
          wpn.SetLevelIndex(1);
        }
        else if (asset == "needle")
        {
          wpn.SetLevelIndex(2);
        }
        else if (asset == "basler")
        {
          wpn.SetLevelIndex(3);
        }
        else if (asset == "dagger")
        {
          wpn.SetLevelIndex(4);
        }
        else if (asset == "flamberge_dagger")
        {
          wpn.SetLevelIndex(6);
        }
        else if (asset == "guard_dagger")
        {
          wpn.SetLevelIndex(8);
        }
        else if (asset == "war_dagger")
        {
          wpn.SetLevelIndex(11);
        }
        else
          ReportError("unknown asset: " + asset);

        FinishWeaponInit(asset, wpn);
        return wpn;
      };

      var names = new[] { "stabber", "needle", "basler", "dagger",  "flamberge_dagger", "guard_dagger", "war_dagger" };
      foreach (var name in names)
        factory[name] = createWpn;
    }

    void CreateBashing()
    {
      Func<string, Weapon> createWpn = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Bashing;

        if (asset == "club")
        {
          wpn.SetLevelIndex(1);
        }
        else if (asset == "spiked_club")
        {
          wpn.SetLevelIndex(3);
        }
        else if (asset == "enhanced_club")
        {
          wpn.SetLevelIndex(4);
        }
        else if (asset == "power_club")
        {
          wpn.SetLevelIndex(6);
        }
        else if (asset == "hammer")
        {
          wpn.SetLevelIndex(8);
        }
        else if (asset == "solid_hammer")
        {
          wpn.SetLevelIndex(10);
        }
        else if (asset == "war_hammer")
        {
          wpn.SetLevelIndex(12);
        }
        FinishWeaponInit(asset, wpn);
        return wpn;
      };

      //ancient_club 
      var names = new[] { "club", "spiked_club", "enhanced_club", "power_club", "hammer", "solid_hammer", "war_hammer" };
      foreach (var name in names)
        factory[name] = createWpn;
    }

    void CreateSwords()
    {
      Func<string, Weapon> createWpn = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Sword;

        if (asset == "rusty_sword")
        {
          wpn.SetLevelIndex(1);
        }
        else if (asset == "rapier")
        {
          wpn.SetLevelIndex(2);
        }
        else if (asset == "gladius")
        {
          wpn.SetLevelIndex(3);
        }
        else if (asset == "sabre")
        {
          wpn.SetLevelIndex(4);
        }
        else if (asset == "viking_sword")
        {
          wpn.SetLevelIndex(5);
        }
        else if (asset == "broad_sword")
        {
          wpn.SetLevelIndex(6);
        }
        else if (asset == "bastard_sword")
        {
          wpn.SetLevelIndex(8);
        }
        else if (asset == "flamberge")
        {
          wpn.SetLevelIndex(10);
        }
        else if (asset == "war_sword")
        {
          wpn.SetLevelIndex(12);
        }
        FinishWeaponInit(asset, wpn);
        return wpn;
      };

      var names = new[] { "rusty_sword", "rapier", "gladius", "sabre", "viking_sword", "broad_sword", "bastard_sword", "flamberge", "war_sword" };
      foreach (var name in names)
        factory[name] = createWpn;
    }

    private static void FinishWeaponInit(string asset, Weapon wpn)
    {
      if (asset.Contains("_level"))
      {
        asset = asset.Substring(0, asset.IndexOf("_level"));
      }
      wpn.tag1 = asset;
      wpn.SetNameFromTag1();

      if (wpn.LevelIndex <= 0)
        throw new Exception("wpn.LevelIndex <= 0 "+wpn);
      if (wpn.Damage <= 0)
        throw new Exception("wpn.Damage <= 0 " + wpn);
    }

    protected override void Create()
    {
      Func<string, Weapon> createWpn = (string asset) =>
      {
        var w = new Tiles.Looting.Equipment.Weapon();
        w.tag1 = asset;
        w.SetNameFromTag1();

        return w;
      };

      CreateMagical();
      CreateSwords();
      CreateAxes();
      CreateBashing();
      CreateDaggers();
      CreateCrossbows();
      CreateBows();

      CreateUniqueWeapons();

      CreatePrototypes();
      //UniqueItemTags = prototypes.Where(i => i.Value.Class == Roguelike.Tiles.EquipmentClass.Unique).Select(i => i.Value.tag1).ToList();
    }

    private void CreateUniqueWeapons()
    {
      Func<string, Weapon> createBlindJustice = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.tag1 = asset;
        wpn.Kind = WeaponKind.Sword;
        wpn.Damage = 5;
        wpn.Name = "Blind Justice";
        wpn.Kind = WeaponKind.Sword;
        var es = new EntityStats();
        es.SetFactor(EntityStatKind.ColdAttack, 3);
        es.SetFactor(EntityStatKind.FireAttack, 1);
        es.SetFactor(EntityStatKind.LifeStealing, 4);
        es.SetFactor(EntityStatKind.ChanceToMeleeHit, 5);
        //es.SetFactor(EntityStatKind.ChanceToStrikeBack, 10);//TODO
        wpn.SetUnique(es, 2);
        return wpn;
      };
      factory["blind_justice"] = createBlindJustice;

      Func<string, Weapon> createShark = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.tag1 = asset;
        wpn.Kind = WeaponKind.Sword;
        wpn.Damage = 4;
        wpn.Name = "shark";
        
        wpn.Kind = WeaponKind.Sword;
        var es = new EntityStats();
        es.SetFactor(EntityStatKind.ColdAttack, 2);
        es.SetFactor(EntityStatKind.FireAttack, 2);
        es.SetFactor(EntityStatKind.PoisonAttack, 2);
        es.SetFactor(EntityStatKind.ChanceToMeleeHit, 10);
        //wpn.SetLevelIndex(2);
        //es.SetFactor(EntityStatKind.ChanceToStrikeBack, 10);//TODO
        wpn.SetUnique(es, 2);
        return wpn;
      };
      factory["shark"] = createShark;

      Func<string, Weapon> createTusk = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Dagger;
        wpn.tag1 = asset;
        wpn.Damage = 6;
        wpn.Name = "Tusk";
        
        var es = new EntityStats();
        es.SetFactor(EntityStatKind.PoisonAttack, 6);
        es.SetFactor(EntityStatKind.LifeStealing, 4);
        es.SetFactor(EntityStatKind.ChanceToMeleeHit, 5);
        //es.SetFactor(EntityStatKind.ChanceToStrikeBack, 10);//TODO
        wpn.SetUnique(es, 3);


        var req = new EntityStats();
        req.SetStat(EntityStatKind.Dexterity, 15);
        wpn.RequiredStats = req;

        return wpn;
      };

      factory["tusk"] = createTusk;

      Func<string, Weapon> createPiS = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Sword;
        wpn.tag1 = asset;
        wpn.Damage = 15;
        wpn.Name = "Law & Justice";
        
        var es = new EntityStats();
        es.SetFactor(EntityStatKind.Strength, 30);
        es.SetFactor(EntityStatKind.MeleeAttack, -30);
        wpn.SetUnique(es, 4);
        wpn.Price = 500;
        return wpn;
      };

      factory["PiS"] = createPiS;

      Func<string, Weapon> createHarvester = (string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Axe;
        wpn.tag1 = asset;
        wpn.Damage = 8;
        wpn.Name = "Harvester";
        wpn.Price = 100;
        var es = new EntityStats();
        es.SetFactor(EntityStatKind.ColdAttack, 5);
        es.SetFactor(EntityStatKind.LifeStealing, 3);
        es.SetFactor(EntityStatKind.ChanceToCauseBleeding, 10);
        es.SetFactor(EntityStatKind.ChanceToMeleeHit, 5);
        es.SetFactor(EntityStatKind.ChanceToCauseTearApart, 15);
        wpn.SetUnique(es, 4);
        return wpn;
      };
      factory["Harvester"] = createHarvester;

      factory["swiatowid_sword"] = ((string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Sword;
        wpn.tag1 = asset;
        wpn.DisplayedName = "Światowid's Sword";
        wpn.Damage = 18;
        wpn.Name = asset;
        wpn.Price = 60;

        var es = new EntityStats();
        es.SetFactor(EntityStatKind.ColdAttack, 5);
        es.SetFactor(EntityStatKind.FireAttack, 5);
        es.SetFactor(EntityStatKind.PoisonAttack, 5);
        es.SetFactor(EntityStatKind.ChanceToMeleeHit, 10);
        es.SetFactor(EntityStatKind.LifeStealing, 4);
        //es.SetFactor(EntityStatKind.LightPower, BiggerLightPowerPerc);
        wpn.SetUnique(es, 6);

        var req = new EntityStats();
        req.SetStat(EntityStatKind.Strength, 20);
        wpn.RequiredStats = req;
        return wpn;
      });

      factory["crusher"] = ((string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Bashing;
        wpn.tag1 = asset;
        wpn.DisplayedName = "Crusher";
        wpn.Damage = 30;
        wpn.Name = asset;
        wpn.Price = 80;

        var es = new EntityStats();
        es.SetFactor(EntityStatKind.ColdAttack, 5);
        es.SetFactor(EntityStatKind.FireAttack, 5);
        es.SetFactor(EntityStatKind.PoisonAttack, 5);
        es.SetFactor(EntityStatKind.ChanceToCauseStunning, 10);
        //es.SetFactor(EntityStatKind.ChanceToMeleeHit, 10);
        //es.SetFactor(EntityStatKind.LifeStealing, 4);
        //es.SetFactor(EntityStatKind.LightPower, BiggerLightPowerPerc);
        wpn.SetUnique(es, 8);

        var req = new EntityStats();
        req.SetStat(EntityStatKind.Strength, 30);
        wpn.RequiredStats = req;
        return wpn;
      });

      factory["Kafar"] = ((string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Bashing;
        wpn.tag1 = asset;
        wpn.DisplayedName = "Kafar";
        wpn.Damage = 8;
        wpn.Name = asset;
        wpn.Price = 30;

        var es = new EntityStats();
        es.SetFactor(EntityStatKind.MeleeAttack, 4);
        es.SetFactor(EntityStatKind.ChanceToMeleeHit, 5);
        es.SetFactor(EntityStatKind.LifeStealing, 2);
        es.SetFactor(EntityStatKind.ChanceToCauseStunning, 10);

        wpn.SetUnique(es, 3);

        var req = new EntityStats();
        req.SetStat(EntityStatKind.Strength, 20);
        wpn.RequiredStats = req;
        return wpn;
      });

      factory["Doomspike"] = ((string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Dagger;
        wpn.tag1 = asset;
        wpn.DisplayedName = "Doomspike";
        wpn.Damage = 7;
        wpn.Name = asset;
        wpn.Price = 30;

        var es = new EntityStats();
        es.SetFactor(EntityStatKind.ColdAttack, 3);
        es.SetFactor(EntityStatKind.ChanceToStrikeBack, 10);
        es.SetFactor(EntityStatKind.ChanceToCauseBleeding, 10);

        wpn.SetUnique(es, 3);

        var req = new EntityStats();
        req.SetStat(EntityStatKind.Dexterity, 20);
        wpn.RequiredStats = req;
        return wpn;
      });

      factory["Barbaric Piercer"] = ((string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Dagger;
        wpn.tag1 = asset;
        wpn.DisplayedName = "Barbaric Piercer";
        wpn.Damage = 8;
        wpn.Name = asset;
        wpn.Price = 30;

        var es = new EntityStats();
        es.SetFactor(EntityStatKind.MeleeAttack, 3);
        es.SetFactor(EntityStatKind.ChanceToMeleeHit, 5);
        es.SetFactor(EntityStatKind.LifeStealing, 2);
        es.SetFactor(EntityStatKind.ChanceToCauseBleeding, 10);

        wpn.SetUnique(es, 5);

        var req = new EntityStats();
        req.SetStat(EntityStatKind.Dexterity, 25);
        wpn.RequiredStats = req;
        return wpn;
      });

      factory["Death'sKiss"] = ((string asset) =>
      {
        var wpn = new Tiles.Looting.Equipment.Weapon();
        wpn.Kind = WeaponKind.Dagger;
        wpn.tag1 = asset;
        wpn.DisplayedName = "Death'sKiss";
        wpn.Damage = 13;
        wpn.Name = asset;
        wpn.Price = 50;

        var es = new EntityStats();
        es.SetFactor(EntityStatKind.FireAttack, 5);
        es.SetFactor(EntityStatKind.ChanceToBulkAttack, 10);
        es.SetFactor(EntityStatKind.ChanceToCauseBleeding, 15);

        wpn.SetUnique(es, 7);

        var req = new EntityStats();
        req.SetStat(EntityStatKind.Dexterity, 30);
        wpn.RequiredStats = req;
        return wpn;
      });
    }
        
  }
}
