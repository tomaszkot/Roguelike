using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.LootFactories;
using Roguelike.Probability;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public Looting Probability { get => probability; set => probability = value; }
    public EquipmentFactory EquipmentFactory { get => equipmentFactory; }
    public int LevelIndex { get; internal set; } = -1;

    public LootGenerator()
    {
      CreateEqFactory();
      var lootSourceKinds = Enum.GetValues(typeof(LootSourceKind)).Cast<LootSourceKind>();

      var lootingChancesForEqEnemy = new EquipmentClassChances();
      lootingChancesForEqEnemy.SetValue(EquipmentClass.Plain, .1f);
      lootingChancesForEqEnemy.SetValue(EquipmentClass.Magic, .05f);
      lootingChancesForEqEnemy.SetValue(EquipmentClass.MagicSecLevel, .033f);
      lootingChancesForEqEnemy.SetValue(EquipmentClass.Unique, .01f);

      var lootKinds = Enum.GetValues(typeof(LootKind)).Cast<LootKind>();
      foreach (var lootSource in lootSourceKinds)
      {
        if (lootSource == LootSourceKind.Enemy)
          probability.SetLootingChance(lootSource, lootingChancesForEqEnemy);
        else
        {
          var lootingChancesForEq = CreateLootingChancesForEquipmentClass(lootSource, lootingChancesForEqEnemy);
          probability.SetLootingChance(lootSource, lootingChancesForEq);
        }

        foreach(var lk in lootKinds)
        {
          var lkChance = new LootKindChances();
          lkChance.SetChance(lk, .2f);
          probability.SetLootingChance(lootSource, lkChance);
        }
      }
    }

    protected virtual void CreateEqFactory()
    {
      equipmentFactory = new EquipmentFactory();
    }

    EquipmentClassChances CreateLootingChancesForEquipmentClass
    (
      LootSourceKind lootSourceKind,
      EquipmentClassChances enemy
      )
    {
      if (lootSourceKind == LootSourceKind.Barrel)
      {
        return enemy.Clone(.5f);
      }
      else
      {
        var lootingChancesForEq = new EquipmentClassChances();
        if (lootSourceKind == LootSourceKind.DeluxeGoldChest ||
          lootSourceKind == LootSourceKind.GoldChest)
        {
          lootingChancesForEq.SetValue(EquipmentClass.Unique, 1);
        }
        else if (lootSourceKind == LootSourceKind.PlainChest)
        {
          return enemy.Clone(1);
        }
        else
          Debug.Assert(false);

        return lootingChancesForEq;
      }
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
      var eq = equipmentFactory.GetRandom(kind);
      EnasureLevelIndex(eq);
      return eq;
    }

    private void EnasureLevelIndex(Equipment eq)
    {
      if (eq != null)
        eq.SetLevelIndex(LevelIndex);
    }

    internal Loot TryGetRandomLootByDiceRoll(LootSourceKind lsk)
    {
      LootKind lootKind = LootKind.Unset;
      if (lsk == LootSourceKind.DeluxeGoldChest ||
        lsk == LootSourceKind.GoldChest 
        //lsk == LootSourceKind.PlainChest
        )
      {
        if (lsk != LootSourceKind.PlainChest)
          lootKind = LootKind.Equipment;
      }
      else
        lootKind = Probability.RollDiceForKind(lsk);
           
      if (lootKind == LootKind.Equipment)
      {
        var eqClass = Probability.RollDice(lsk);
        if (eqClass != EquipmentClass.Unset)
        {
          var item = GetRandomEquipment(eqClass);
          return item;
        }
      }

      if (lootKind == LootKind.Unset)
        return null;//lootKind = RandHelper.GetRandomEnumValue<LootKind>(true);

      return GetRandomLoot(lootKind);
    }

    protected virtual Equipment GetRandomEquipment(EquipmentClass eqClass)
    {
      var randedEnum = RandHelper.GetRandomEnumValue<EquipmentKind>(new[] { EquipmentKind.TrophyLeft, EquipmentKind.TrophyRight, EquipmentKind.Unset });
      return equipmentFactory.GetRandom(randedEnum, eqClass);
    }

    public virtual Loot GetRandomLoot(LootKind kind)
    {
      Loot res = null;
      //if(kind == LootKind.Potion)
      //  return  new PotionKind
      if (kind == LootKind.Gold)
        res = new Gold();
      else if (kind == LootKind.Equipment)
        res = GetRandomEquipment(EquipmentClass.Plain);
      else
        res = GetRandomLoot();

      if(res is Equipment)
        EnasureLevelIndex(res as Equipment);
      return res;
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
