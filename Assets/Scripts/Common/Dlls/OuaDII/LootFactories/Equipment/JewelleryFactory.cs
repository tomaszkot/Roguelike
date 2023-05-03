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
  class JewelleryFactory : EquipmentTypeFactory
  {
    const int StartRingStatValue = 2;
    const int StartAmuletStatValue = 3;
    const int MaxLevel = 30;


    public JewelleryFactory(Container cont) : base(cont)
    {
    }

    Roguelike.Tiles.Looting.Jewellery createJewellery(Roguelike.Tiles.EquipmentKind kind, string tag1)
    {
      var juwell = new Roguelike.Tiles.Looting.Jewellery();
      juwell.EquipmentKind = kind;
      juwell.tag1 = tag1;
      juwell.Price = 10;
      return juwell;
    }

    protected override void Create()
    {
      CreateJewelleries();
    }

    void CreateJewelleries()
    {
      CreateAmulets();

      CreateRings();

      CreatePrototypes();
    }

    protected override Roguelike.Tiles.Loot GetRandromFromPrototype(int level)
    {
      var loot = GetRandomFromAll() as Roguelike.Tiles.Looting.Jewellery;// base.GetRandromFromPrototype(level) 
      if (loot != null)
      {
        SetStats(level, loot, loot.EquipmentKind == Roguelike.Tiles.EquipmentKind.Ring);
      }
      return loot;
    }

    int CalcStatValue(Roguelike.Tiles.Looting.Jewellery juw, int level)
    {
      var start = juw.EquipmentKind == Roguelike.Tiles.EquipmentKind.Amulet ? StartAmuletStatValue : StartRingStatValue;
      var val = start + level;
      return val;
    }

    private void CreateRings()
    {
      Func<string, Roguelike.Tiles.Looting.Jewellery> createRing = (string asset) =>
      {
        var juw = createJewellery(Roguelike.Tiles.EquipmentKind.Ring, asset);

        return juw;
      };

      var names = new[] { "ring_magic", "ring_defense", "ring_attack" };
      foreach (var name in names)
        factory[name] = createRing;
    }

    private void SetStats(int level, Roguelike.Tiles.Looting.Jewellery juw, bool ring)
    {
      juw.SetLevelIndex(level);
      var val = CalcStatValue(juw, level);
      var asset = juw.tag1;

      if (ring)
      {
        if (asset == "ring_magic")
        {
          juw.SetPrimaryStat(EntityStatKind.Magic, val);
        }
        if (asset == "ring_defense")
        {
          juw.SetPrimaryStat(EntityStatKind.Defense, val);
        }
        if (asset == "ring_attack")
        {
          juw.SetPrimaryStat(EntityStatKind.MeleeAttack, val);
        }
      }
      else
      {
        if (asset == "amulet_of_attack")
        {
          juw.SetPrimaryStat(EntityStatKind.MeleeAttack, val);
        }
        else if (asset == "amulet_of_magic")
        {
          juw.SetPrimaryStat(EntityStatKind.Magic, val);
        }
        else if (asset == "amulet_of_defense")
        {
          juw.SetPrimaryStat(EntityStatKind.Defense, val);
        }
      }
    }

    private void CreateAmulets()
    {
      Func<string, Roguelike.Tiles.Looting.Jewellery> createAmulet = (string asset) =>
      {
        var amulet = createJewellery(Roguelike.Tiles.EquipmentKind.Amulet, asset);
        return amulet;
      };
      var amulets = new[] { "amulet_of_attack", "amulet_of_magic", "amulet_of_defense" };
      foreach (var amulet in amulets)
        factory[amulet] = createAmulet;
    }
  }
}
