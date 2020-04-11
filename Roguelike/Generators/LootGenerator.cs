using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.LootFactories;
using Roguelike.Probability;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;

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
    protected EquipmentFactory equipmentFactory;
    Dictionary<string, Loot> uniqueLoot = new Dictionary<string, Loot>();
    Looting probability = new Looting();

    public Looting Probability { get => probability; }
    public EquipmentFactory EquipmentFactory { get => equipmentFactory; }

    public LootGenerator()
    {
      CreateEqFactory();
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

    protected virtual void CreateEqFactory()
    {
      equipmentFactory = new EquipmentFactory();
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
        
    public virtual Loot GetLootByName(string tileName)
    {
      if (uniqueLoot.ContainsKey(tileName))
        return uniqueLoot[tileName];

      return null;
    }

    public virtual T GetLootByTileName<T>(string tileName) where T : Loot
    {
      return GetLootByName(tileName) as T;
    }

    public virtual Equipment GetRandom(EquipmentKind kind)
    {
      return equipmentFactory.GetRandom(kind);
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
          var item = GetRandomEquipment(eqClass);
          //if (eqClass == EquipmentClass.Magic)
          //  item.MakeMagic();
          //else if (eqClass == EquipmentClass.Unique)
          //{
            
          //}
          return item;
        }
      }

      return GetRandomLoot(lootKind);
    }

    protected virtual Equipment GetRandomEquipment(EquipmentClass eqClass)
    {
      var randedEnum = RandHelper.GetRandomEnumValue<EquipmentKind>(new[] { EquipmentKind.TrophyLeft, EquipmentKind.TrophyRight, EquipmentKind.Unset });
      return equipmentFactory.GetRandom(randedEnum, eqClass);
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
