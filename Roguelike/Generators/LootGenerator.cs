using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Attributes;
using Roguelike.Probability;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Generators
{
  public class EqEntityStats
  {
    EntityStats es = new EntityStats();
    public EntityStats Get()
    {
      return es;
    }

    public EqEntityStats Add(EntityStatKind sk, int val)
    {
      es.SetFactor(sk, val);
      return this;
    }
  }

  public class LootGenerator
  {
    Dictionary<string, Loot> uniqueLoot = new Dictionary<string, Loot>();
    Looting probability = new Looting();

    public LootGenerator()
    {
      var lootSourceKinds = Enum.GetValues(typeof(LootSourceKind));
      var lootingChancesForEqEnemy = new LootingChancesForEquipmentClass();
      foreach (var lootSource in lootSourceKinds.Cast<LootSourceKind>())
      {
        if (lootSource == LootSourceKind.Enemy)
          probability.SetLootingChance(lootSource, lootingChancesForEqEnemy);
        else
        {
          var lootingChancesForEq = CreateLootingChancesForEquipmentClass(lootSource, lootingChancesForEqEnemy);
          probability.SetLootingChance(lootSource, lootingChancesForEq);
        }
      }
    }

    LootingChancesForEquipmentClass CreateLootingChancesForEquipmentClass
    (
      LootSourceKind lootSourceKind,
      LootingChancesForEquipmentClass enemy
      )
    {

      var lootingChancesForEq = new LootingChancesForEquipmentClass();
      if (lootSourceKind == LootSourceKind.Barrel)
      {
        lootingChancesForEq.MagicItem = lootingChancesForEq.MagicItem / 2;
        lootingChancesForEq.SecLevelMagicItem = lootingChancesForEq.SecLevelMagicItem / 2;
        lootingChancesForEq.UniqueItem = lootingChancesForEq.UniqueItem / 2;
      }
      else
      {
        if (lootSourceKind == LootSourceKind.DeluxeGoldChest ||
          lootSourceKind == LootSourceKind.GoldChest)
        {
          lootingChancesForEq.MagicItem = 0;
          lootingChancesForEq.SecLevelMagicItem = 0;
          lootingChancesForEq.UniqueItem = 1;
        }
      }

      return lootingChancesForEq;
    }

    public Looting Probability { get => probability; }

    public virtual Loot GetLootByTileName(string tileName)
    {
      if (uniqueLoot.ContainsKey(tileName))
        return uniqueLoot[tileName];

      return null;
    }

    public virtual T GetLootByTileName<T>(string tileName) where T : Loot
    {
      return GetLootByTileName(tileName) as T;
    }

    public virtual Equipment GetRandom(EquipmentKind kind)
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
        case EquipmentKind.RingLeft:
          eq = GetRandomJewellery(EntityStatKind.Attack, EquipmentKind.RingLeft);
          break;
        case EquipmentKind.RingRight:
          eq = GetRandomJewellery(EntityStatKind.Attack, EquipmentKind.RingRight);
          break;
        case EquipmentKind.Amulet:
          eq = GetRandomJewellery(EntityStatKind.Attack, EquipmentKind.Amulet);
          break;
        case EquipmentKind.TrophyLeft:
          break;
        case EquipmentKind.TrophyRight:
          break;
        case EquipmentKind.Gloves:
          eq = GetRandomGloves();

          break;
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

    internal Loot TryGetRandomLootByDiceRoll(LootSourceKind lsk)
    {
      LootKind lootKind = LootKind.Unset;
      if (lsk == LootSourceKind.DeluxeGoldChest ||
        lsk == LootSourceKind.GoldChest ||
        lsk == LootSourceKind.PlainChest)
      {
        if (lsk == LootSourceKind.PlainChest)
        {
          //TODO
        }
        else
          lootKind = LootKind.Equipment;
      }
      else
        lootKind = Probability.RollDiceForKind(lsk);

      if (lootKind == LootKind.Unset)
        return null;

      if (lootKind == LootKind.Equipment)
      {
        var eqClass = Probability.RollDice(lsk);
        if (eqClass != EquipmentClass.Unset)
        {
          var randedEnum = RandHelper.GetRandomEnumValue<EquipmentKind>(new[] { EquipmentKind.TrophyLeft, EquipmentKind .TrophyRight, EquipmentKind .Unset});
          var item = GetRandom(randedEnum);
          if (eqClass == EquipmentClass.Magic)
            item.MakeMagic();
          else if (eqClass == EquipmentClass.Unique)
          {
            var ees = new EqEntityStats();
            ees.Add(EntityStatKind.Health, 15)
            .Add(EntityStatKind.Attack, 15)
            .Add(EntityStatKind.Defence, 15)
            .Add(EntityStatKind.ChanceToCastSpell, 15);
            item.SetUnique(ees.Get());
          }
          return item;
        }
      }

      return GetRandomLoot(lootKind);
    }

    protected virtual Loot GetRandomLoot(LootKind lootKind, EquipmentClass eqClass)
    {
      //var rand = GetRandom(lootKind);
      return null;
    }

    public virtual Weapon GetRandomWeapon()
    {
      var item = new Weapon();
      item.Name = "Sword";
      item.Kind = Weapon.WeaponKind.Sword;
      item.LootKind = LootKind.Equipment;
      item.EquipmentKind = EquipmentKind.Weapon;
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
      var item = new Equipment(EquipmentKind.Gloves);
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
      var jew = createJewellery(EquipmentKind.RingLeft, minDropDungeonLevel);
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

    public virtual Loot GetRandomLoot(LootKind kind)
    {
      //if(kind == LootKind.Potion)
      //  return  new PotionKind
      if (kind == LootKind.Gold)
        return new Gold();
      return GetRandomLoot();
    }

    public virtual Loot GetRandomLoot()
    {
      var loot = new Mushroom();
      loot.SetKind(MushroomKind.Boletus);
      loot.tag1 = "mash3";
      return loot;
    }

    public virtual Loot GetRandomStackedLoot()
    {
      var loot = new Food();
      loot.SetKind(FoodKind.Plum);
      loot.tag1 = "plum_mirabelka";
      return loot;
    }
  }
}
