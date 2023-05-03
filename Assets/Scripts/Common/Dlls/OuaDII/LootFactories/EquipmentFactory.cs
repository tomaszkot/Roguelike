using Dungeons.Core;
using OuaDII.LootFactories.Equipment;
using OuaDII.Tiles.Looting.Equipment;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.LootFactories;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.LootFactories
{
  public class EquipmentFactory : Roguelike.LootFactories.EquipmentFactory
  {
    List<string> reservedUniqItems = new List<string>() { "Kafar" };
    HoundEqFactory houndEqFactory;

    public EquipmentFactory(Container container) : base(container)
    {
    }

    protected override void CreateKindFactories()
    {
      var eqKinds = Enum.GetValues(typeof(Roguelike.Tiles.EquipmentKind)).Cast<Roguelike.Tiles.EquipmentKind>();

      lootCreators[Roguelike.Tiles.EquipmentKind.Weapon] = new WeaponFactory(container);

      var jf = new JewelleryFactory(container);
      lootCreators[Roguelike.Tiles.EquipmentKind.Ring] = jf;
      lootCreators[Roguelike.Tiles.EquipmentKind.Amulet] = jf;

      var af = new ArmorFactory(container);
      lootCreators[Roguelike.Tiles.EquipmentKind.Armor] = af;
      lootCreators[Roguelike.Tiles.EquipmentKind.Helmet] = af;
      lootCreators[Roguelike.Tiles.EquipmentKind.Glove] = af;
      lootCreators[Roguelike.Tiles.EquipmentKind.Shield] = af;

      houndEqFactory = new HoundEqFactory(this.container);
    }

    public override Roguelike.Tiles.Looting.Equipment GetRandom(EquipmentKind kind, int maxEqLevel, Roguelike.Tiles.EquipmentClass eqClass = EquipmentClass.Plain)
    {
      Roguelike.Tiles.Looting.Equipment eq = null;
      if (eqClass == EquipmentClass.Plain && RandHelper.GetRandomDouble() < 0.1f)
      {
        eq = houndEqFactory.GetRandom(kind, maxEqLevel) as Roguelike.Tiles.Looting.Equipment;
      }

      if (eq == null)
      {
        if (lootCreators.ContainsKey(kind))
        {
          if (eqClass == EquipmentClass.Unique)
          {
            var uniqOnes = lootCreators[kind].GetUniqueItems(maxEqLevel);
            if (this.lootHistory != null)
            {
              uniqOnes.RemoveAll(i => this.lootHistory.GeneratedLoot.Any(hist => hist.Tag1 == i.tag1));
              uniqOnes.RemoveAll(i => reservedUniqItems.Any(j => j == i.Name));
            }
            var proto = RandHelper.GetRandomElem(uniqOnes);
            if (proto != null)
              eq = lootCreators[kind].GetByAsset(proto.tag1) as Roguelike.Tiles.Looting.Equipment;
          }

          if (eq == null)
          {
            eq = lootCreators[kind].GetRandom(maxEqLevel) as Roguelike.Tiles.Looting.Equipment;
          }
        }
      }
      if (eq.Class != EquipmentClass.Unique)
      {
        var _eqClass = eqClass == EquipmentClass.Unique ? EquipmentClass.MagicSecLevel : eqClass;
        if (_eqClass != EquipmentClass.Plain)
          MakeMagic(_eqClass, eq);
      }

      return eq;
    }
  }
}
