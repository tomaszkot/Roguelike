using Dungeons;
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
      lootFactory = cont.GetInstance<AbstractLootFactory>() as LootFactory;
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
          DebugHelper.Assert(false);

        return lootingChancesForEq;
      }
    }

    protected virtual void PrepareLoot(Loot loot)
    {
      //adjust price...

    }

    public virtual Loot GetLootByAsset(string tileAsset)
    {
      Loot loot;
      if (uniqueLoot.ContainsKey(tileAsset))
        loot = uniqueLoot[tileAsset];
      else
        loot = LootFactory.GetByAsset(tileAsset);

      if (tileAsset == "cap")
      {
        var arm = new Armor();
        arm.EquipmentKind = Roguelike.Tiles.EquipmentKind.Helmet;
        arm.Defense = 5;
        arm.tag1 = tileAsset;
        arm.SetLevelIndex(1);
        return arm;
      }
      tileAsset = tileAsset.ToLower();

      if (loot == null)
      {
        var wpn = new Weapon();
        loot = wpn;
        if (tileAsset == "rusty_sword")
        {
          wpn.Kind = Weapon.WeaponKind.Sword;
          wpn.tag1 = "rusty_sword";
          wpn.Name = "Rusty sword";
          wpn.SetLevelIndex(1);
        }
        else if (tileAsset == "sickle")
        {
          wpn.Kind = Weapon.WeaponKind.Axe;
          wpn.tag1 = "sickle";
          wpn.Name = "Sickle";
          wpn.SetLevelIndex(1);
          loot = wpn;
        }
        else if (tileAsset == "axe")
        {
          wpn.Kind = Weapon.WeaponKind.Axe;
          wpn.tag1 = "axe";
          wpn.Name = "Axe";
          wpn.SetLevelIndex(2);
          loot = wpn;
        }

        else if (tileAsset == "gladius")
        {
          wpn.Kind = Weapon.WeaponKind.Sword;
          wpn.tag1 = "gladius";
          wpn.Name = "Gladius";
          wpn.SetLevelIndex(3);
          wpn.Price *= 2;
          
        }

        else if (tileAsset == "hammer")
        {
          wpn.Kind = Weapon.WeaponKind.Bashing;
          wpn.tag1 = "hammer";
          wpn.Name = "hammer";
          wpn.Price *= 2;
          wpn.SetLevelIndex(4);
        }
        else if (tileAsset == "broad_sword")
        {
          wpn.Kind = Weapon.WeaponKind.Sword;
          wpn.tag1 = "broad_sword";
          wpn.Name = "broad_sword";
          wpn.Price *= 2;
          wpn.SetLevelIndex(6);
        }
        else if (tileAsset == "war_dagger")
        {
          wpn.Kind = Weapon.WeaponKind.Dagger;
          wpn.tag1 = "war_dagger";
          wpn.Name = "War Dagger";
          wpn.Price *= 2;
          wpn.SetLevelIndex(5);
        }

        else if (tileAsset == "scepter")
        {
          wpn.Kind = Weapon.WeaponKind.Scepter;
          wpn.tag1 = "scepter";
          wpn.Name = "Scepter";
          wpn.Price *= 2;
          wpn.SetLevelIndex(1);

        }
        else if (tileAsset == "staff")
        {
          wpn.Kind = Weapon.WeaponKind.Staff;
          wpn.tag1 = "staff";
          wpn.Name = "Staff";
          wpn.Price *= 2; 
          wpn.SetLevelIndex(1);

        }
        else if (tileAsset == "wand")
        {
          wpn.Kind = Weapon.WeaponKind.Wand;
          wpn.tag1 = "wand";
          wpn.Name = "Wand";
          wpn.Price *= 2;
          wpn.SetLevelIndex(1);
        }

        else if (tileAsset == "bow")
        {
          wpn.Kind = Weapon.WeaponKind.Bow;
          wpn.tag1 = "bow";
          wpn.Damage = Props.BowBaseDamage;
          wpn.Name = "Bow";
          wpn.Price *= 2;
          wpn.SetLevelIndex(1);
        }

        else if (tileAsset == "crossbow")
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
      try
      {
        var eqClass = EquipmentClass.Plain;
        if (ab != null && ab.ExtraChanceToGetMagicLoot > RandHelper.GetRandomDouble())
          eqClass = EquipmentClass.Magic;
        var eq = LootFactory.EquipmentFactory.GetRandom(kind, level, eqClass);
        //EnasureLevelIndex(eq);//level must be given by factory!
        return eq;
      }
      catch (Exception ex)
      {
        Container.GetInstance<ILogger>().LogError(ex);
        return GetErrorEqPh();
      }
    }

    private Equipment GetErrorEqPh()
    {
      //must return null = much of code rely on it now
      return null;// LootFactory.GetByName("rusty_sword") as Equipment;
    }

    bool debug = false;

    internal Loot TryGetRandomLootByDiceRoll(LootSourceKind lsk, int maxEqLevel, LootAbility ab)
    {
      try
      {
        if (debug)
          return GetRandomEquipment(EquipmentKind.Weapon, maxEqLevel);

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
      catch (Exception ex)
      {
        Container.GetInstance<ILogger>().LogError(ex);
        return new MagicDust();
      }
    }

    protected virtual Equipment GetRandomEquipment(EquipmentClass eqClass, int level)
    {
      try
      {
        var randedEnum = GetPossibleEqKind();

        LootFactory.EquipmentFactory.lootHistory = this.lootHistory;
        var generatedEq = LootFactory.EquipmentFactory.GetRandom(randedEnum, level, eqClass);
        if (generatedEq == null || (generatedEq.Class != EquipmentClass.Unique && eqClass == EquipmentClass.Unique))
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
      catch (Exception ex)
      {
        Container.GetInstance<ILogger>().LogError(ex);
        return GetErrorEqPh(); 
      }
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

      if (powerKind == EnemyPowerKind.Champion || powerKind == EnemyPowerKind.Boss)
      {
        if (eq.Class == EquipmentClass.Plain && !eq.Enchantable)
          enchant = true;
        else if (eq.Class == EquipmentClass.Magic)
        {
          eq.PromoteToSecondMagicClass();
          if (eq.Class == EquipmentClass.Magic)
          {
            int k = 0;
              k++;
          }
        }
      }
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
      return LootFactory.EquipmentFactory.GetRandom(EquipmentKind.Amulet, 1);
    }

    public virtual Loot GetRandomRing()
    {
      return LootFactory.EquipmentFactory.GetRandom(EquipmentKind.Ring, 1);
    }

    static string[] GemTags;

    public virtual Loot GetRandomLoot(LootKind kind, int level)
    {
      try
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
          var enumVal = RandHelper.GetRandomEnumValue<FoodKind>();
          if (enumVal == FoodKind.Mushroom)
            res = new Mushroom(RandHelper.GetRandomEnumValue<MushroomKind>());
          else
            res = new Food(RandHelper.GetRandomEnumValue<FoodKind>(new[] { FoodKind.Unset, FoodKind.Mushroom }));//Mushroom is a diff type
        }
        else if (kind == LootKind.Plant)
          res = new Plant(RandHelper.GetRandomEnumValue<PlantKind>());
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
          DebugHelper.Assert(false);
        }
        PrepareLoot(res);
        return res;
      }
      catch (Exception ex)
      {
        Container.GetInstance<ILogger>().LogError(ex);
        return new MagicDust();
      }
    }

    List<RecipeKind> alreadyGen = new List<RecipeKind>();
    protected virtual Recipe GetRandRecipe()
    {
      var kind_ = RandHelper.GetRandomEnumValue<RecipeKind>();
      if (alreadyGen.Contains(kind_))
        kind_ = RandHelper.GetRandomEnumValue<RecipeKind>();//help luck
      var res = new Recipe(kind_);
      if(!alreadyGen.Contains(kind_))
        alreadyGen.Add(kind_);
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
      //  DebugHelper.Assert(false);

      var lootName = RandHelper.GetRandomElem<string>(tags);
      res = GetLootByAsset(lootName);
      return res;
    }

    private static string[] CreateGemTags(List<string> gemTags)
    {
      var kinds = EnumHelper.Values<GemKind>(true);
      var sizes = EnumHelper.Values<EnchanterSize>(true);
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
      if(debug)
        return GetRandomEquipment(EquipmentKind.Weapon, level);

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

    public List<Weapon> GetWeapons(Weapon.WeaponKind crossbow, int level)
    {
      return LootFactory.EquipmentFactory.GetWeapons(crossbow, level);

    }
  }
}
