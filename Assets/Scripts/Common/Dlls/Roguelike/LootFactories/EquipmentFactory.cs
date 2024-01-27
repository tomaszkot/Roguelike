using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.History;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.LootFactories
{
  public class Props
  {
    public const int BowBaseDamage = 10;//strength is not added to it so melee would be stronger
    public const int CrossbowBaseDamage = 12;

    public const int ScepterBaseDamage = 2;
    public const int StaffBaseDamage = 3;
    public const int WandBaseDamage = 2;

    public const int FightItemBaseDamage = 1;

  }

  public class MaterialProps
  {
    public const int BashingWeaponBaseDamage = 3;
    public const int BronzeSwordBaseDamage = 3;
    public const int BronzeDaggerBaseDamage = 2;
    public const int BronzeAxeBaseDamage = 3;

    public const int BronzeToIronMult = 2;
    public const int BronzeToSteelMult = 3;

    public const int IronDropLootSrcLevel = 3;
    public const int SteelDropLootSrcLevel = 6;
    
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

    //public override Roguelike.Tiles.Loot GetByName(string assetName)
    //{
    //  foreach (var kv in lootCreators)
    //  {
    //    var tile = kv.Value.GetByName(assetName);
    //    if (tile != null)
    //      return tile;
    //  }
    //  return null;
    //}

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

    //transient obj
    LootHistory uniqLootHistory = new LootHistory();

    public virtual Equipment GetRandom(EquipmentKind kind, int maxEqLevel, EquipmentClass eqClass = EquipmentClass.Plain)
    {
      Equipment eq = null;

      if (eqClass == EquipmentClass.Unique)
      {
        if (this.lootCreators.Any())
        {
          var eqTag = this.lootCreators[EquipmentKind.Weapon].GetUniqueItems(maxEqLevel).FirstOrDefault();
          eq = GetByAsset(eqTag) as Equipment;
        }
        if (eq == null)
        {
          eq = GetRandomWeapon();//TODO
          eq.SetClass(EquipmentClass.Unique, maxEqLevel);
          eq.SetLevelIndex(maxEqLevel);
        }

        if (eq.LevelIndex <= 0)
          throw new Exception("eq.LevelIndex <= 0!");
        return eq;
      }

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
          eq = GetRandomJewellery(EntityStatKind.MeleeAttack, EquipmentKind.Ring);
          break;
        case EquipmentKind.Amulet:
          eq = GetRandomJewellery(EntityStatKind.MeleeAttack, EquipmentKind.Amulet);
          break;
        case EquipmentKind.Trophy:
          break;
        case EquipmentKind.Glove:
          eq = GetRandomGloves();

          break;
      }
      if (eq != null)
      {
        if (eq.Class == EquipmentClass.Plain && eqClass != EquipmentClass.Plain)
        {
          MakeMagic(eqClass, eq);
        }
      }
      eq.SetLevelIndex(maxEqLevel);

      return eq;
    }

    protected void MakeMagic(EquipmentClass eqClass, Equipment eq)
    {
      if (eq.Class == EquipmentClass.Unique)
        return;
      if (eq.IsPlain())
        eq.MakeMagic();
      if (eqClass == EquipmentClass.MagicSecLevel)
        eq.PromoteToSecondMagicClass();
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

      item.PrimaryStatKind = EntityStatKind.MeleeAttack;
      item.PrimaryStatValue = 5;
      return item;
    }

    public virtual List<Weapon> GetWeapons(Weapon.WeaponKind kind, int level)
    {
      return new List<Weapon>();
    }

    public virtual Weapon GetRandomWeapon(Weapon.WeaponKind kind)
    {
      var item = new Weapon();
      if (kind == Weapon.WeaponKind.Sword)
      {
        item.Name = "Sword";
        item.tag1 = "Sword";
        item.Kind = Weapon.WeaponKind.Sword;

        item.PrimaryStatKind = EntityStatKind.MeleeAttack;
        item.PrimaryStatValue = 5;
        return item;
      }

      return null;
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
      jew.tag1 = "ring_of_" + sk;

      if (sk == EntityStatKind.ResistCold || sk == EntityStatKind.ResistFire || sk == EntityStatKind.ResistPoison)
      {
        jew.Name += " resistance";
      }
      jew.SetLevelIndex(minDropDungeonLevel);

      return jew;
    }

    protected virtual List<PrototypeValue> GetPlainsAtLevel(int level, EquipmentKind ek,  bool materialAware, ref bool fakeDecreaseOfLevel)
    {
      return lootCreators[ek].GetPlainsAtLevel(level, materialAware, true, ref fakeDecreaseOfLevel);
    }
  }
}
