using Roguelike.Attributes;
using Roguelike.Generators;
using Roguelike.Tiles;
using SimpleInjector;
using System.Collections.Generic;

namespace Roguelike.LootFactories
{
  public class EquipmentFactory : AbstractLootFactory
  {
    protected Dictionary<EquipmentKind, EquipmentTypeFactory> lootCreators = new Dictionary<EquipmentKind, EquipmentTypeFactory>();

    public EquipmentFactory(Container container) : base(container)
    {
    }

    public override Loot GetByTag(string tagPart)
    {
      foreach (var kv in lootCreators)
      {
        var tile = kv.Value.GetByTag(tagPart);
        if (tile != null)
          return tile;
      }
      return null;
    }

    public override Roguelike.Tiles.Loot GetByName(string assetName)
    {
      foreach (var kv in lootCreators)
      {
        var tile = kv.Value.GetByName(assetName);
        if (tile != null)
          return tile;
      }
      return null;
    }

    public void SetFactory(EquipmentKind ek, EquipmentTypeFactory factory)
    {
      lootCreators[ek] = factory;
    }

    protected virtual void CreateKindFactories()
    {
    }

    public override Loot GetRandom()
    {
      return null;
    }

    protected override void Create()
    {
      CreateKindFactories();
    }

    public virtual Equipment GetRandom(EquipmentKind kind, EquipmentClass eqClass = EquipmentClass.Plain)
    {
      Equipment eq = null;
      
      switch (kind)
      {
        case EquipmentKind.Unset:
          break;
        case EquipmentKind.Weapon:
          eq = GetRandomWeapon();
          break;
        case EquipmentKind.Armor:
          eq = GetRandomArmor();
          break;
        case EquipmentKind.Helmet:
          eq = GetRandomHelmet();
          break;
        case EquipmentKind.Shield:
          eq = GetRandomShield();
          break;
        case EquipmentKind.Ring:
          eq = GetRandomJewellery(EntityStatKind.Attack, EquipmentKind.Ring);
          break;
        case EquipmentKind.Amulet:
          eq = GetRandomJewellery(EntityStatKind.Attack, EquipmentKind.Amulet);
          break;
        case EquipmentKind.Trophy:
          break;
        case EquipmentKind.Glove:
          eq = GetRandomGloves();

          break;
      }
      if (eq != null && eqClass != EquipmentClass.Plain)
      {
        if (eqClass == EquipmentClass.Plain)
          eq.MakeMagic();
        else
        {
          //TODO
          var ees = new EqEntityStats();
          ees.Add(EntityStatKind.Health, 15)
          .Add(EntityStatKind.Attack, 15)
          .Add(EntityStatKind.Defence, 15)
          .Add(EntityStatKind.ChanceToCastSpell, 15);
          eq.SetUnique(ees.Get());
        }
      }
      
      return eq;
    }

    private Equipment GetRandomArmor()
    {
      var item = new Equipment(EquipmentKind.Armor);
      item.Name = "Armor";
      item.PrimaryStatKind = EntityStatKind.Defence;
      item.PrimaryStatValue = 3;
      return item;
    }
    
    public virtual Weapon GetRandomWeapon()
    {
      var item = new Weapon();
      item.Name = "Sword";
      item.Kind = Weapon.WeaponKind.Sword;

      item.PrimaryStatKind = EntityStatKind.Attack;
      item.PrimaryStatValue = 5;
      return item;
    }

    public virtual Equipment GetRandomHelmet()
    {
      var item = new Equipment(EquipmentKind.Helmet);
      item.Name = "Helmet";
      //item.Kind = Weapon.WeaponKind.Sword;
      item.PrimaryStatKind = EntityStatKind.Defence;
      item.PrimaryStatValue = 2;
      return item;
    }

    public virtual Equipment GetRandomShield()
    {
      var item = new Equipment(EquipmentKind.Shield);
      item.Name = "Buckler";
      item.PrimaryStatKind = EntityStatKind.Defence;
      item.PrimaryStatValue = 1;
      return item;
    }

    public virtual Equipment GetRandomGloves()
    {
      var item = new Equipment(EquipmentKind.Glove);
      item.Name = "Gloves";
      item.PrimaryStatKind = EntityStatKind.Defence;
      item.PrimaryStatValue = 1;
      return item;
    }

    public virtual Jewellery GetRandomJewellery(EntityStatKind sk, EquipmentKind eq = EquipmentKind.Unset)
    {
      if (eq == EquipmentKind.Amulet)
        return createAmulet(sk, 1, 3);
      return AddRing("", sk, 1, 3);
    }

    Jewellery createJewellery(EquipmentKind kind, int minDropDungeonLevel)
    {
      var juwell = new Jewellery();
      juwell.EquipmentKind = kind;
      juwell.MinDropDungeonLevel = minDropDungeonLevel;
      juwell.Price = 10;
      return juwell;
    }

    private Jewellery createAmulet(EntityStatKind sk, int minDungeonLevel, int statValue)
    {
      var jew = createJewellery(EquipmentKind.Amulet, minDungeonLevel);
      jew.tag1 = sk.ToString() + "_amulet";
      int AmuletStatAddition = 2;
      jew.SetPrimaryStat(sk, statValue + AmuletStatAddition);

      var name = "amulet of ";// "amulet of ";
      jew.Name = name + sk.ToString();
      if (sk == EntityStatKind.ResistCold || sk == EntityStatKind.ResistFire || sk == EntityStatKind.ResistPoison)
      {
        jew.Name += " resistance";
      }

      return jew;
    }

    private Jewellery AddRing(string asset, EntityStatKind sk, int minDropDungeonLevel,
      int statValue)
    {
      var jew = createJewellery(EquipmentKind.Ring, minDropDungeonLevel);
      jew.tag1 = asset;
      //juw.ExtendedInfo.Stats.SetFactor(EntityStatKind.ResistCold, 10);
      jew.SetPrimaryStat(sk, statValue);
      var name = "ring of ";
      jew.Name = name + sk;

      if (sk == EntityStatKind.ResistCold || sk == EntityStatKind.ResistFire || sk == EntityStatKind.ResistPoison)
      {
        jew.Name += " resistance";
      }
      jew.MinDropDungeonLevel = minDropDungeonLevel;

      return jew;
    }
  }
}
