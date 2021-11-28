using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.History;
using Roguelike.LootFactories;
using Roguelike.Probability;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
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
    LootFactory lootFactory;
    Dictionary<string, Loot> uniqueLoot = new Dictionary<string, Loot>();
    Looting probability = new Looting();

    public Looting Probability { get => probability; set => probability = value; }
    public int LevelIndex
    {
      get;
      set;
    } = -1;

    [JsonIgnore]
    public Container Container { get; set; }
    public LootFactory LootFactory { get => lootFactory; set => lootFactory = value; }

    public LootGenerator(Container cont)
    {
      Container = cont;
      lootFactory = cont.GetInstance<LootFactory>();
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
          var val = .25f;
          var mult = 1f;

          if (lk == LootKind.Equipment)
          {
            mult = 1.3f;
          }
          //if (lootSource == LootSourceKind.PlainChest || lootSource == LootSourceKind.Barrel)
          {
            if (lk == LootKind.Gem || lk == LootKind.HunterTrophy)
              mult /= 3f;
          }

          if (lk == LootKind.Potion || lk == LootKind.Scroll)
          {
            mult = 1.3f;
          }

          val *= mult;

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

    protected virtual void PrepareLoot(Loot loot)
    {
      //adjust price...

    }

    public virtual Loot GetLootByAsset(string tileName)
    {
      Loot loot;
      if (uniqueLoot.ContainsKey(tileName))
        loot = uniqueLoot[tileName];
      else
        loot = LootFactory.GetByName(tileName);

      tileName = tileName.ToLower();

      if (loot == null)
      {
        var wpn = new Weapon();
        loot = wpn;
        if (tileName == "rusty_sword")
        {
          wpn.Kind = Weapon.WeaponKind.Sword;
          wpn.tag1 = "rusty_sword";
          wpn.Name = "Rusty sword";
          wpn.SetLevelIndex(1);
        }
        else if (tileName == "axe")
        {
          wpn.Kind = Weapon.WeaponKind.Axe;
          wpn.tag1 = "axe";
          wpn.Name = "Axe";
          wpn.SetLevelIndex(2);
          loot = wpn;
        }

        else if (tileName == "gladius")
        {
          wpn.Kind = Weapon.WeaponKind.Sword;
          wpn.tag1 = "gladius";
          wpn.Name = "Gladius";
          wpn.SetLevelIndex(3);
          wpn.Price *= 2;
          
        }

        else if (tileName == "hammer")
        {
          wpn.Kind = Weapon.WeaponKind.Bashing;
          wpn.tag1 = "hammer";
          wpn.Name = "hammer";
          wpn.Price *= 2;
          wpn.SetLevelIndex(4);
        }

        else if (tileName == "war_dagger")
        {
          wpn.Kind = Weapon.WeaponKind.Dagger;
          wpn.tag1 = "war_dagger";
          wpn.Name = "War Dagger";
          wpn.Price *= 2;
          wpn.SetLevelIndex(5);
        }

        else if (tileName == "scepter")
        {
          wpn.Kind = Weapon.WeaponKind.Scepter;
          wpn.tag1 = "scepter";
          wpn.Name = "Scepter";
          wpn.Price *= 2;
          wpn.SetLevelIndex(1);

        }
        else if (tileName == "staff")
        {
          wpn.Kind = Weapon.WeaponKind.Staff;
          wpn.tag1 = "staff";
          wpn.Name = "Staff";
          wpn.Price *= 2; 
          wpn.SetLevelIndex(1);

        }
        else if (tileName == "wand")
        {
          wpn.Kind = Weapon.WeaponKind.Wand;
          wpn.tag1 = "wand";
          wpn.Name = "Wand";
          wpn.Price *= 2;
          wpn.SetLevelIndex(1);
        }

        else if (tileName == "bow")
        {
          wpn.Kind = Weapon.WeaponKind.Bow;
          wpn.tag1 = "bow";
          wpn.Damage = Props.BowBaseDamage;
          wpn.Name = "Bow";
          wpn.Price *= 2;
          wpn.SetLevelIndex(1);
        }

        else if (tileName == "crossbow")
        {
          wpn.Kind = Weapon.WeaponKind.Crossbow;
          wpn.tag1 = "crossbow";
          wpn.Damage = Props.CrossbowBaseDamage;
          wpn.Name = "Bow";
          wpn.Price *= 2;
          wpn.SetLevelIndex(1);
        }
      }
      var wpnRes = loot as Weapon;
      if (wpnRes != null && wpnRes.LevelIndex <=0)
        wpnRes.SetLevelIndex(1);
      return loot;
    }

    public virtual T GetLootByTileName<T>(string tileName) where T : Loot
    {
      return GetLootByAsset(tileName) as T;
    }

    public virtual Equipment GetRandomEquipment(int maxEqLevel, LootAbility ab)
    {
      var levelToUse = maxEqLevel > 0 ? maxEqLevel : (LevelIndex + 1);

      if (levelToUse <= 0)
        Container.GetInstance<ILogger>().LogError("GetRandomEquipment levelToUse <=0!!!");
      var kind = GetPossibleEqKind();
      return GetRandomEquipment(kind, levelToUse, ab);
    }

    public virtual Equipment GetRandomEquipment(EquipmentKind kind, int level, LootAbility ab = null)
    {
      var eqClass = EquipmentClass.Plain;
      if (ab != null && ab.ExtraChanceToGetMagicLoot > RandHelper.GetRandomDouble())
        eqClass = EquipmentClass.Magic;
      var eq = LootFactory.EquipmentFactory.GetRandom(kind, level, eqClass);
      //EnasureLevelIndex(eq);//level must be given by factory!
      return eq;
    }



    internal Loot TryGetRandomLootByDiceRoll(LootSourceKind lsk, int maxEqLevel, LootAbility ab)
    {
      //return null;
      LootKind lootKind = LootKind.Unset;
      if (
        lsk == LootSourceKind.DeluxeGoldChest ||
        lsk == LootSourceKind.GoldChest
        )
      {
        lootKind = LootKind.Equipment;
      }
      else if (lsk == LootSourceKind.PlainChest)
        return GetRandomLoot(maxEqLevel);//some cheap loot
      else
        lootKind = Probability.RollDiceForKind(lsk, ab);

      if (lootKind == LootKind.Equipment)
      {
        var eqClass = Probability.RollDice(lsk, ab);
        if (eqClass != EquipmentClass.Unset)
        {
          var item = GetRandomEquipment(eqClass, maxEqLevel);
          //if (item is Equipment eq)
          // {
          //   EnsureMaterialFromLootSource(eq);
          //   if (item.LevelIndex < maxEqLevel)
          //   {
          //     //int k = 0;
          //     //k++;
          //   }
          // }
          return item;
        }
      }

      if (lootKind == LootKind.Unset)
        return null;

      return GetRandomLoot(lootKind, maxEqLevel);
    }

    protected virtual Equipment GetRandomEquipment(EquipmentClass eqClass, int level)
    {
      var randedEnum = GetPossibleEqKind();

      LootFactory.EquipmentFactory.lootHistory = this.lootHistory;
      var generatedEq = LootFactory.EquipmentFactory.GetRandom(randedEnum, level, eqClass);
      if ((generatedEq == null || generatedEq.Class != EquipmentClass.Unique) && eqClass == EquipmentClass.Unique)
      {
        var values = GetEqKinds();
        foreach (var kind in values)
        {
          generatedEq = LootFactory.EquipmentFactory.GetRandom(kind, level, eqClass);
          if (generatedEq != null)
            break;
        }
      }

      return generatedEq;
    }

    public static List<EquipmentKind> GetEqKinds()
    {
      var skip = new[] { EquipmentKind.Trophy, EquipmentKind.God, EquipmentKind.Unset };
      var values = Enum.GetValues(typeof(EquipmentKind)).Cast<EquipmentKind>().Where(i => !skip.Contains(i)).ToList();
      return values;
    }

    protected LootHistory lootHistory;
    public virtual Loot GetBestLoot(EnemyPowerKind powerKind, int level, LootHistory lootHistory, Abilities.LootAbility ab)
    {
      this.lootHistory = lootHistory;
      EquipmentClass eqClass = EquipmentClass.Plain;
      bool enchant = false;
      if (powerKind == EnemyPowerKind.Boss)
        eqClass = EquipmentClass.Unique;
      else if (powerKind == EnemyPowerKind.Champion)
      {
        var threshold = 0.85f;
        threshold -= ab.ExtraChanceToGetUniqueLoot;
        if (RandHelper.GetRandomDouble() > threshold)
          eqClass = EquipmentClass.Unique;
        else
        {
          bool alwaysEnchantable = false;
          if (!alwaysEnchantable && RandHelper.GetRandomDouble() > 0.5)
            eqClass = EquipmentClass.MagicSecLevel;
          else
          {
            eqClass = EquipmentClass.Plain;
            enchant = true;
          }
        }
      }
      var eq = GetRandomEquipment(eqClass, level);
      if (enchant)
        eq.MakeEnchantable(2);
      return eq;
    }

    public EquipmentKind ForcedEquipmentKind { get; set; }
    private EquipmentKind GetPossibleEqKind()
    {
      if (ForcedEquipmentKind != EquipmentKind.Unset)
      {
        return ForcedEquipmentKind;
      }
      return RandHelper.GetRandomEnumValue<EquipmentKind>(new[] { EquipmentKind.Trophy, EquipmentKind.God, EquipmentKind.Unset });
    }

    public virtual Loot GetRandomJewellery()
    {
      return LootFactory.EquipmentFactory.GetRandom(EquipmentKind.Amulet, -1);
    }

    public virtual Loot GetRandomRing()
    {
      return LootFactory.EquipmentFactory.GetRandom(EquipmentKind.Ring, -1);
    }

    static string[] GemTags;

    public virtual Loot GetRandomLoot(LootKind kind, int level)
    {
      Loot res = null;

      if (kind == LootKind.Gold)
        res = new Gold();
      else if (kind == LootKind.Equipment)
        res = GetRandomEquipment(EquipmentClass.Plain, level);
      else if (kind == LootKind.Potion)
        res = GetRandomPotion();
      else if (kind == LootKind.Food)
      {
        var enumVal = RandHelper.GetRandomEnumValue<FoodKind>(true);
        if (enumVal == FoodKind.Mushroom)
          res = new Mushroom();
        else
          res = new Food();
      }
      else if (kind == LootKind.Plant)
        res = new Plant();
      else if (kind == LootKind.Scroll)
      {
        var scroll = LootFactory.ScrollsFactory.GetRandom(level) as Scroll;
        var rand = RandHelper.GetRandomDouble();

        if ((scroll.Kind == Spells.SpellKind.Portal && rand > 0.2f) //no need for so many of them
          || (scroll.Kind != Spells.SpellKind.Identify && rand > 0.6f)) //these are fine
        {
          var newScroll = LootFactory.ScrollsFactory.GetRandom(level) as Scroll;
          //if (newScroll.Kind != Spells.SpellKind.Portal || scroll.Kind == Spells.SpellKind.Portal)
          scroll = newScroll;
        }

        res = scroll;
      }
      else if (kind == LootKind.Book)
      {
        res = LootFactory.BooksFactory.GetRandom(level) as Book;
      }
      else if (kind == LootKind.Gem)
      {
        res = GetRandomEnchanter(level, false);
        //var lootName = RandHelper.GetRandomElem<string>(GemTags.ToArray());
        //res = GetLootByName(lootName);
      }
      else if (kind == LootKind.HunterTrophy)
      {
        res = GetRandomEnchanter(level, true);
      }
      else if (kind == LootKind.Recipe)
      {
        res = GetRandRecipe();
      }
      else if (kind == LootKind.FightItem)
      {
        res = LootFactory.MiscLootFactory.GetRandomFightItem(level);
      }
      else if (kind == LootKind.Other)
      {
        var rand = RandHelper.GetRandomDouble();
        if (rand > 0.5f)
          res = new MagicDust();
        else
          res = new Hooch();
      }
      else
      {
        //LootFactory.GetRandom()
        Debug.Assert(false);
      }

      //if(res is Equipment)
      //        EnasureLevelIndex(res as Equipment);
      return res;
    }

    protected virtual Recipe GetRandRecipe()
    {
      var kind_ = RandHelper.GetRandomEnumValue<RecipeKind>();
      var res = new Recipe(kind_);
      return res;
    }

    string getEnchanterPreffix(int level)
    {
      if (level <= 5)
      {
        return Enchanter.Small;
      }
      if (level <= 10)
      {
        return Enchanter.Medium;
      }
      return Enchanter.Big;
    }

    private Loot GetRandomEnchanter(int level, bool tinyTrophy)
    {
      Loot res = null;
      var preff = getEnchanterPreffix(level);
      List<string> tags = null;
      if (tinyTrophy)
        tags = HunterTrophy.TinyTrophiesTags.Where(i => i.StartsWith(preff)).ToList();
      else
      {
        var gemTags = new List<string>();
        if (GemTags == null)
        {
          GemTags = CreateGemTags(gemTags);
        }
        tags = GemTags.Where(i => i.EndsWith(preff)).ToList();
      }
      //else
      //  Debug.Assert(false);

      var lootName = RandHelper.GetRandomElem<string>(tags);
      res = GetLootByAsset(lootName);
      return res;
    }

    private static string[] CreateGemTags(List<string> gemTags)
    {
      var kinds = RandHelper.GetEnumValues<GemKind>(true);
      var sizes = RandHelper.GetEnumValues<EnchanterSize>(true);
      foreach (var kind_ in kinds)
      {
        foreach (var size in sizes)
        {
          gemTags.Add(Gem.CalcTagFrom(kind_, size));
        }
      }
      return gemTags.ToArray();
    }

    private Loot GetRandomPotion()
    {
      var enumVal = RandHelper.GetRandomEnumValue<PotionKind>(new[] { PotionKind.Special, PotionKind.Unset });
      var potion = new Potion();
      potion.SetKind(enumVal);
      return potion;
    }

    //a cheap loot generated randomly on the level
    public virtual Loot GetRandomLoot(int level, LootKind skip = LootKind.Unset)
    {
      if (RandHelper.GetRandomDouble() > .9f)//TODO
      {
        return new Hooch();
      }
      var enumVal = RandHelper.GetRandomEnumValue<LootKind>(new[]
      {
        LootKind.Other, 
        //LootKind.Gem, LootKind.Recipe, LootKind.HunterTrophy
        LootKind.Seal, LootKind.SealPart, LootKind.Unset, skip, LootKind.Book
      });
      var loot = GetRandomLoot(enumVal, level);
      return loot;
    }


  }
}
