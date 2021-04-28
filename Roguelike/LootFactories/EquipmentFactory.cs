using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Generators;
using Roguelike.History;
using Roguelike.Tiles;
using SimpleInjector;
using System.Collections.Generic;

namespace Roguelike.LootFactories
{
  public class MaterialProps
  {
    public const int BashingWeaponBaseDamage = 1;

    public const int BronzeSwordBaseDamage = 2;
    public const int BronzeDaggerBaseDamage = 2;
    public const int BronzeAxeBaseDamage = 2;
    //public readonly Dictionary<Roguelike.Tiles.EquipmentKind, int> BronzeArmorBaseDefence = new Dictionary<Roguelike.Tiles.EquipmentKind, int>()
    //{
    //  {EquipmentKind.Armor, 12} //ironed_leather_armor has 10
    //  //{Roguelike.Tiles.EquipmentKind.Shield, 0 }
    //};

    public const int BronzeToIronMult = 2;
    public const int BronzeToSteelMult = 3;

    public const int IronDropLootSrcLevel = 3;
    public const int SteelDropLootSrcLevel = 4;
  }

  public class EquipmentFactory : AbstractLootFactory
  {
    protected Dictionary<EquipmentKind, EquipmentTypeFactory> lootCreators = new Dictionary<EquipmentKind, EquipmentTypeFactory>();
    internal protected LootHistory lootHistory;

    public EquipmentFactory(Container container) : base(container)
    {
    }

    public override IEnumerable<Loot> GetAll()
    {
      List<Loot> loot = new List<Loot>();
      foreach (var lc in lootCreators)
        loot.AddRange(lc.Value.GetAll());

      return loot;
    }

    public override Loot GetRandom(int level)
    {
      var index = RandHelper.GetRandomEnumValue<EquipmentKind>(new[] { EquipmentKind.God, EquipmentKind.Trophy, EquipmentKind.Unset });
      var lootCreator = lootCreators[index];
      return lootCreator.GetRandom(level);
    }

    public override Loot GetByAsset(string tagPart)
    {
      foreach (var kv in lootCreators)
      {
        var tile = kv.Value.GetByAsset(tagPart);
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

    protected override void Create()
    {
      CreateKindFactories();
    }

    public virtual Equipment GetRandom(EquipmentKind kind, int maxEqLevel, EquipmentClass eqClass = EquipmentClass.Plain)
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
        {
          MakeMagic(eqClass, eq);
        }
        else
        {
          //TODO
          var ees = new EqEntityStats();
          ees.Add(EntityStatKind.Health, 15)
          .Add(EntityStatKind.Attack, 15)
          .Add(EntityStatKind.Defense, 15)
          .Add(EntityStatKind.ChanceToCastSpell, 15);
          eq.SetUnique(ees.Get(), 5);
        }
      }
      eq.SetLevelIndex(maxEqLevel);//TODO 
      return eq;
    }

    protected void MakeMagic(EquipmentClass eqClass, Equipment eq)
    {
      if (eq.Class == EquipmentClass.Unique)
        return;
      if (eq.IsPlain())
        eq.MakeMagic();
      if (!eq.IsSecondMagicLevel && eqClass == EquipmentClass.MagicSecLevel)
      {
        var stats = eq.GetPossibleMagicStats();
        var stat = RandHelper.GetRandomElem(stats);
        eq.MakeMagicSecLevel(stat.Key, 3);//TODO
      }
    }

    public virtual Equipment GetRandomArmor()
    {
      var item = new Equipment(EquipmentKind.Armor);
      item.Name = "Armor";
      item.tag1 = "Armor";
      item.PrimaryStatKind = EntityStatKind.Defense;
      item.PrimaryStatValue = 3;
      return item;
    }

    public virtual Weapon GetRandomWeapon()
    {
      var item = new Weapon();
      item.Name = "Sword";
      item.tag1 = "Sword";
      item.Kind = Weapon.WeaponKind.Sword;

      item.PrimaryStatKind = EntityStatKind.Attack;
      item.PrimaryStatValue = 5;
      return item;
    }

    public virtual Equipment GetRandomHelmet()
    {
      var item = new Equipment(EquipmentKind.Helmet);
      item.Name = "Helmet";
      item.tag1 = "Helmet";
      //item.Kind = Weapon.WeaponKind.Sword;
      item.PrimaryStatKind = EntityStatKind.Defense;
      item.PrimaryStatValue = 2;
      return item;
    }

    public virtual Equipment GetRandomShield()
    {
      var item = new Equipment(EquipmentKind.Shield);
      item.Name = "Buckler";
      item.tag1 = "??";
      item.PrimaryStatKind = EntityStatKind.Defense;
      item.PrimaryStatValue = 1;
      return item;
    }

    public virtual Equipment GetRandomGloves()
    {
      var item = new Equipment(EquipmentKind.Glove);
      item.Name = "Gloves";
      item.tag1 = "??";
      item.PrimaryStatKind = EntityStatKind.Defense;
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
      juwell.SetLevelIndex(minDropDungeonLevel);
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
      jew.tag1 = "amulet_of_" + sk;
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
      jew.tag1 = "ring_" + sk;

      if (sk == EntityStatKind.ResistCold || sk == EntityStatKind.ResistFire || sk == EntityStatKind.ResistPoison)
      {
        jew.Name += " resistance";
      }
      jew.SetLevelIndex(minDropDungeonLevel);

      return jew;
    }
  }
}
