using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
//using static UnityEngine.EventSystems.EventTrigger;

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

  public class SpellState
  {
    public SpellKind Kind { get; set; }
    public int Level { get; set; } = 1;

    public int CoolDownCounter { get; set; } = 0;

    internal bool IsCoolingDown()
    {
      return CoolDownCounter > 0;
    }

    [JsonIgnore]
    public string LastIncError { get; set; }

    public int MaxLevel = 10;

    public bool IncreaseLevel(IAdvancedEntity entity)
    {

      //if (abilityLevelToPlayerLevel.ContainsKey(Level + 1))
      //{
      //  var lev = abilityLevelToPlayerLevel[Level + 1];
      //  if (lev > entity.Level)
      //  {
      //    LastIncError = "Required character level for ability increase: " + lev;
      //    return false;
      //  }
      //}
      if (CanIncLevel(entity))
      {
        Level++;
        //SetStatsForLevel();
        return true;
      }
      return false; 
    }

    public bool CanIncLevel(IAdvancedEntity entity)
    {
      LastIncError = "";
      if (Level == MaxLevel)
      {
        LastIncError = "Max level of the spell reached";
        return false;
      }

      var scroll = new Scroll(Kind);
      var le = entity as LivingEntity;
      var spell = scroll.CreateSpell(le);
      var canInc = le.Stats.Magic >= spell.NextLevelMagicNeeded;
      if (!canInc)
      {
        LastIncError = "Magic level too low";
        return false;
      }
      return true;
    }
  }

  public class SpellStateSet
  {
    Dictionary<SpellKind, SpellState> spellStates = new Dictionary<SpellKind, SpellState>();

    public SpellStateSet()
    {
      var spells = EnumHelper.GetEnumValues<SpellKind>(true);
      foreach (var sp in spells)
      {
        spellStates[sp] = new SpellState() { Kind = sp };
      }
    }

    public SpellState GetState(SpellKind kind)
    {
      return spellStates[kind];
    }
  }

  public class Spell : ISpell
  {
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

        public int ManaCost
    {
      get
      {
        return CalcManaCost(CurrentLevel);
      }
    }

    protected Weapon weaponSpellSource;

    public Spell(LivingEntity caller, Weapon weaponSpellSource)
    {
      this.weaponSpellSource = weaponSpellSource;
      this.Caller = caller;
      manaCost = BaseManaCost;
      levelToMagic[1] = 10;
    }

    protected int CalcManaCost(int level)
    {
      var cost = manaCost + (manaCost * (level - 1)) * manaCostMultiplicator / 100.0f;
      return (int)cost;
    }

    public int CoolingDown { get; set; } = 0;

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

    //TODO ! remove withVariation, use AttackDesc is needed
    public virtual SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel)
    {
      int level = currentMagicLevel ? CurrentLevel : CurrentLevel + 1;
      int? mana = null;
      int? magicRequired = null;
      int range = 0;

      range = GetRange();
      if (weaponSpellSource == null)
      {
        mana = CalcManaCost(level);
        magicRequired = NextLevelMagicNeeded;
      }
      var desc = new SpellStatsDescription(level, mana, magicRequired, Kind, range);
      return desc;
    }

    protected virtual int GetRange()
    {
      var range = 0;
      if (this is IProjectileSpell proj)
        range = proj.Range;
      return range;
    }
  }


}
