using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract.Spells;
using Roguelike.Calculated;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Spells
{
  public enum SpellKind
  {
    Unset = 0, FireBall = 1, CrackedStone = 2, Skeleton = 3, Trap = 4, IceBall = 5, PoisonBall = 6, Transform = 7,
    Frighten = 8, Healing = 9, ManaShield = 10, Telekinesis = 11, StonedBall = 12, LightingBall = 13
        //,MindControl
        , Mana = 14, BushTrap = 15, 
    Weaken = 17, NESWFireBall = 18, Teleport = 19, 
    IronSkin = 20, //TODO move to ability
    ResistAll = 25, 
    Inaccuracy = 29, /*CallMerchant, CallGod,*/ Identify = 30, Portal = 33,
    Dziewanna = 40,
    Swarog = 41,
    Swiatowit = 42,
    FireStone = 50,
    SwapPosition = 53,
    Perun
  }

  public class Spell : ISpell
  {
    Dictionary<AbilityProperty, float?> properties = new Dictionary<AbilityProperty, float?>();
    protected int BaseRange = 3;
    //public int Duration { get; set; }
    public int BaseDuration { get; set; } = 4;
    public int BaseDurability { get; set; } = 40;
    protected float manaCost;
    float manaCostMultiplicator = 20;
    public bool SendByGod { get; set; }
    public SpellKind Kind { get; set; }
    public bool EnemyRequired = false;
    public bool EntityRequired = false;
    public const int BaseManaCost = 4;
    protected Dictionary<int, int> levelToMagic = new Dictionary<int, int>();
    protected LivingEntity caller;
    public bool Utylized { get; set; }
    public bool RequiresDestPoint { get; set; }


    float GetPropertyOrDefault(AbilityProperty prop)
    {
      return GetProperty(prop) ?? 0;
    }

    public int Duration
    {
      get { return (int)GetPropertyOrDefault(AbilityProperty.Duration); }
    }

    public int Range
    {
      get { return (int)GetPropertyOrDefault(AbilityProperty.Range); }
      set { SetProperty(AbilityProperty.Range, value); }
    }

    public int Durability
    {
      get { return (int)GetPropertyOrDefault(AbilityProperty.Durability); }
      set { SetProperty(AbilityProperty.Durability, value); }
    }

    public int ManaCost
    {
      get
      {
        return CalcManaCost(CurrentLevel);
      }
    }

    protected void UnsetProp(AbilityProperty prop)
    {
      SetProperty(prop, null);
    }

    protected void SetProperty(AbilityProperty prop, float? value)
    {
      properties[prop] = value;
    }

    public float? GetProperty(AbilityProperty prop)
    {
      return properties.ContainsKey(prop) ? properties[prop] : 0;
    }

    protected Weapon weaponSpellSource;

    public Spell(LivingEntity caller, Weapon weaponSpellSource, SpellKind sk)
    {
      Kind = sk;
      this.weaponSpellSource = weaponSpellSource;
      this.Caller = caller;
      manaCost = BaseManaCost;
      levelToMagic[1] = 10;

      var ale = caller as AdvancedLivingEntity;
      var en = caller as Enemy;

      if (en != null)
      {
        //TODO
        CurrentLevel = en.Level;

        if (en.PowerKind == EnemyPowerKind.Champion)
          CurrentLevel += 1;
        else if (en.PowerKind == EnemyPowerKind.Boss)
          CurrentLevel += 2;
      }
      else if (ale != null)
      {
        CurrentLevel = ale.Spells.GetState(Kind).Level;
      }
      else if (caller is LivingEntity le)
      {
        CurrentLevel = le.Level;
      }

      var props = EnumHelper.GetEnumValues<AbilityProperty>(true);
      foreach (var prop in props)
      {
        CalcProp(prop, true);
      }
    }

    protected void CalcProp(AbilityProperty prop, bool currentLevel)
    {
      if (prop == AbilityProperty.Duration)
        SetProperty(AbilityProperty.Duration, CalcDuration());
      else if (prop == AbilityProperty.Range)
        SetProperty(AbilityProperty.Range, CalcRange());
      else if (prop == AbilityProperty.Durability)
        SetProperty(AbilityProperty.Durability, CalcDurability());
      else
        throw new Exception("unsupported prop "+ prop);
    }

    protected float? CalcDurability()
    {
      return CalcPropFromLevel(CurrentLevel, BaseDurability);
    }

    protected float? CalcDurability(int level)
    {
      return CalcPropFromLevel(level, BaseDurability);
    }

    public int CalcRange()
    {
      return CalcRange(CurrentLevel);
    }

    public int CalcRange(int level)
    {
      if (GetProperty(AbilityProperty.Range) == null)
        return 0;
      var ra = BaseRange + level;
      return ra;
    }

    protected virtual int CalcDuration(int level)//, float factor = 1)
    {
      //var he = GetHealthFromLevel(level);
      //float baseVal = ((float)he) * factor;
      //return (int)(baseVal / 4f);
      if (GetProperty(AbilityProperty.Duration) == null)
        return 0;
      return BaseDuration + level;
    }

    protected virtual int CalcDuration()//float factor = 1)
    {
      //var level = 1;

      var dur = CalcDuration(CurrentLevel);//, factor);
      //if (CurrentLevel > 1)
      //{
      //  var durPrev = CalcDuration(CurrentLevel-1, factor);
      //  if (durPrev == dur)
      //    dur++; 
      //}
      return dur;
    }

    protected int CalcManaCost(int level)
    {
      var cost = manaCost + (manaCost * (level - 1)) * manaCostMultiplicator / 100.0f;
      return (int)cost;
    }

    public int CoolingDownCounter { get; set; } = 0;

    //[XmlIgnore]
    [JsonIgnore]
    public LivingEntity Caller
    {
      get
      {
        return caller;
      }

      set
      {
        caller = value;
      }
    }

    public int NextLevelMagicNeeded
    {
      get
      {
        var index = GetNextLevelMagicIndex();
        return index < 0 ? index : levelToMagic[index];
      }
    }

    public int GetNextLevelMagicIndex()
    {
      //var ownerMagicAmount = Caller.Stats.GetCurrentValue(EntityStatKind.Magic);
      while (levelToMagic.Count <= currentLevel+1)
      {
        var value = levelToMagic.Values.Last();
        levelToMagic.Add(levelToMagic.Keys.Last() + 1, (int)(value * 1.2));
      }
      //var nextValue = levelToMagic.Values.Where(i => i > ownerMagicAmount).First();
      //var index = levelToMagic.Where(i => i.Value == nextValue).Single().Key;
      //var index = levelToMagic[currentLevel+1];
      return currentLevel + 1;
    }

    public virtual string GetLifetimeSound() { return ""; }
    public virtual string GetHitSound() { return ""; }
    int currentLevel = 1;

    public int CurrentLevel
    {
      get
      {
        if (weaponSpellSource != null)
          return weaponSpellSource.LevelIndex;
        return currentLevel;
      }
      set => currentLevel = value;
    }

    public bool IsFromMagicalWeapon { get => weaponSpellSource != null;}

    protected int CalcPropFromLevel(int lvl, int basePropValue)
    {
      return FactorCalculator.CalcFromLevel(lvl, basePropValue);
    }

    //TODO ! remove withVariation, use AttackDesc is needed
    public virtual SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel)
    {
      int level = currentMagicLevel ? CurrentLevel : CurrentLevel + 1;
      int? mana = null;
      int? magicRequired = null;
      int range = CalcRange(level);

      if (weaponSpellSource == null)
      {
        mana = CalcManaCost(level);
        magicRequired = NextLevelMagicNeeded;
      }

      var desc = new SpellStatsDescription(level, mana, magicRequired, Kind, range);
      if (currentMagicLevel)
      {
        desc.Duration = Duration;
      }
      else
      {
        desc.Duration = CalcDuration(CurrentLevel + 1);
      }
      return desc;
    }
  }


}
