using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.LootFactories;
using Roguelike.Probability;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
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
    bool generateWeaponEarly = true;
    LootFactory lootFactory;
    Dictionary<string, Loot> uniqueLoot = new Dictionary<string, Loot>();
    Looting probability = new Looting();

    public Looting Probability { get => probability; set => probability = value; }
    public int LevelIndex { get; internal set; } = -1;
    public Container Container { get; set; }
    public LootFactory LootFactory { get => lootFactory; set => lootFactory = value; }

    public LootGenerator(Container cont)
    {
      Container = cont;
      lootFactory = cont.GetInstance<LootFactory>();
      //CreateEqFactory();
      var lootSourceKinds = Enum.GetValues(typeof(LootSourceKind)).Cast<LootSourceKind>();

      var lootingChancesForEqEnemy = new EquipmentClassChances();
      lootingChancesForEqEnemy.SetValue(EquipmentClass.Plain, .1f);
      lootingChancesForEqEnemy.SetValue(EquipmentClass.Magic, .05f);
      lootingChancesForEqEnemy.SetValue(EquipmentClass.MagicSecLevel, .033f);
      lootingChancesForEqEnemy.SetValue(EquipmentClass.Unique, .01f);

      var lootKinds = Enum.GetValues(typeof(LootKind)).Cast<LootKind>();

      //iterate chances for: Enemy, Barrel, GoldChest...
      foreach (var lootSource in lootSourceKinds)
      {
        if (lootSource == LootSourceKind.Enemy)
          probability.SetLootingChance(lootSource, lootingChancesForEqEnemy);
        else
        {
          var lootingChancesForEq = CreateLootingChancesForEquipmentClass(lootSource, lootingChancesForEqEnemy);
          probability.SetLootingChance(lootSource, lootingChancesForEq);
        }

        //2 set Loot Kind chances
        var lkChance = new LootKindChances();
        foreach (var lk in lootKinds)
        {
          var val = .2f;
          if (lk == LootKind.Potion)
            val *= 1.5f;
          lkChance.SetChance(lk, val);
        }
        probability.SetLootingChance(lootSource, lkChance);
      }
    }

    protected virtual void CreateEqFactory()
    {
      //EquipmentTypeFactory wpns = new EquipmentTypeFactory();
    }

    EquipmentClassChances CreateLootingChancesForEquipmentClass
    (
      LootSourceKind lootSourceKind,
      EquipmentClassChances eqClassChances
      )
    {
      if (lootSourceKind == LootSourceKind.Barrel)
      {
        return eqClassChances.Clone(.5f);
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
          return eqClassChances.Clone(1);
        }
        else
          Debug.Assert(false);

        return lootingChancesForEq;
      }
    }

    public virtual Loot GetLootByTag(string tagPart)
    {
      var loot = LootFactory.GetByTag(tagPart) as Roguelike.Tiles.Loot;
      if (loot != null)
        PrepareLoot(loot);
      return loot;
    }

    protected virtual void PrepareLoot(Loot loot)
    {
      //adjust price...

    }

    public virtual Loot GetLootByName(string tileName)
    {
      Loot loot;
      if (uniqueLoot.ContainsKey(tileName))
        loot = uniqueLoot[tileName];
      else
        loot = LootFactory.GetByName(tileName);
      if (loot == null && tileName == "rusty_sword")
      {
        var wpn = new Weapon();
        wpn.tag1 = "rusty_sword";
        wpn.Damage = 2;
        wpn.Name = "Rusty sword";
        return wpn;
      }

      if (loot == null && tileName == "gladius")
      {
        var wpn = new Weapon();
        wpn.tag1 = "gladius";
        wpn.Damage = 5;
        wpn.Name = "Gladius";
        wpn.Price *= 2;
        return wpn;
      }

      return loot;
    }

    public virtual T GetLootByTileName<T>(string tileName) where T : Loot
    {
      return GetLootByName(tileName) as T;
    }

    public virtual Equipment GetRandomEquipment()
    {
      if(LevelIndex <=0)
        Container.GetInstance<ILogger>().LogError("GetRandomEquipment LevelIndex <=0!!!");
      return GetRandomEquipment(LevelIndex);
    }

    public virtual Equipment GetRandomEquipment(int level)
    {
      var kind = GetPossibleEqKind();
      return GetRandomEquipment(kind, level);
    }

    public virtual Equipment GetRandomEquipment(EquipmentKind kind)
    {
      if (LevelIndex <= 0)
        Container.GetInstance<ILogger>().LogError("GetRandomEquipment (kind) LevelIndex <=0!!!");

      return GetRandomEquipment(kind, LevelIndex);
    }

    public virtual Equipment GetRandomEquipment(EquipmentKind kind, int level)
    {
      //TODO level!
      var eq = LootFactory.EquipmentFactory.GetRandom(kind);
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
      //return null;
      if (generateWeaponEarly)
      { 
        
      }

      LootKind lootKind = LootKind.Unset;
      if (lsk == LootSourceKind.DeluxeGoldChest ||
        lsk == LootSourceKind.GoldChest
        )
      {
        lootKind = LootKind.Equipment;
      }
      else if(lsk == LootSourceKind.PlainChest)
        return GetRandomLoot();//some cheap loot
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
        return null;

      return GetRandomLoot(lootKind);
    }

    protected virtual Equipment GetRandomEquipment(EquipmentClass eqClass)
    {
      var randedEnum = GetPossibleEqKind();
      return LootFactory.EquipmentFactory.GetRandom(randedEnum, eqClass);
    }

    private static EquipmentKind GetPossibleEqKind()
    {
      return RandHelper.GetRandomEnumValue<EquipmentKind>(new[] { EquipmentKind.Trophy, EquipmentKind.God, EquipmentKind.Unset });
    }

    public virtual Loot GetRandomJewellery()
    {
      return LootFactory.EquipmentFactory.GetRandom(EquipmentKind.Amulet);
    }

    public virtual Loot GetRandomRing()
    {
      return LootFactory.EquipmentFactory.GetRandom(EquipmentKind.Ring);
    }

    public virtual Loot GetRandomLoot(LootKind kind)
    {
      Loot res = null;

      if (kind == LootKind.Gold)
        res = new Gold();
      else if (kind == LootKind.Equipment)
        res = GetRandomEquipment(EquipmentClass.Plain);
      else if (kind == LootKind.Potion)
        res = GetRandomPotion();
      else if (kind == LootKind.Food)
      {
        var enumVal = RandHelper.GetRandomEnumValue<FoodKind>(true);
        if(enumVal == FoodKind.Mushroom)
          res = new Mushroom();
        else
          res = new Food();
      }
      else if (kind == LootKind.Plant)
        res = new Plant();
      else if (kind == LootKind.Scroll)
        res = new Scroll(Spells.SpellKind.FireBall);
      else
        res = new Mushroom();

      if(res is Equipment)
        EnasureLevelIndex(res as Equipment);
      return res;
    }

    private Loot GetRandomPotion() //where T : enum
    {
      var enumVal = RandHelper.GetRandomEnumValue<PotionKind>();// (enumKind);
      var potion = new Potion();
      //if (enumVal == PotionKind.Health)
      potion.SetKind(enumVal);
      return potion;
    }

    //a cheap loot generated randomly on the level
    public virtual Loot GetRandomLoot()
    {
      var enumVal = RandHelper.GetRandomEnumValue<LootKind>(new[] 
      { LootKind.Other, LootKind.Gem, LootKind.Recipe, LootKind.Seal, LootKind.SealPart, LootKind.Unset, LootKind.TinyTrophy});
      var loot = GetRandomLoot(enumVal);
      return loot;
    }

    
  }
}
