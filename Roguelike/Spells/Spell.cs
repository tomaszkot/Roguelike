using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public enum FightItemKind
  {
    None,
    ExplodePotion = 1,
    Knife = 2,
    Trap = 3
  }
}

  namespace Roguelike.Spells
{
  public enum SpellKind
  {
    FireBall, CrackedStone, Skeleton, Trap, IceBall, PoisonBall, Transform,
    Frighten, Healing, ManaShield, Telekinesis, StonedBall, LightingBall
        //,MindControl
        , Mana, BushTrap, Rage, Weaken, NESWFireBall, Teleport, IronSkin, ResistAll, Inaccuracy, CallMerchant, CallGod
  }

  public class Spell : IMovingDamager
  {
    protected float manaCost;
    public int CoolingDown;
    float manaCostMultiplicator = 20;
    public bool SendByGod { get; set; }
    public FightItem FightItem { get; internal set; }

    //public int GetExtraChanceForCausingEffect(LivingEntity caster)
    //{
    //  this.Caller = caster;

    //  return 0;//TODO!
    //}

    public int ManaCost
    {
      get
      {
        var level = GetCurrentLevel();
        var cost = manaCost + (manaCost * (level - 1)) * manaCostMultiplicator / 100.0f;
        return (int)cost;
      }
    }
    public SpellKind Kind { get; set; }
    public bool EnemyRequired = false;
    public bool EntityRequired = false;
    public const int BaseManaCost = 4;
    protected Dictionary<int, int> levelToMagic = new Dictionary<int, int>();
    private LivingEntity caller;

    public Spell() : this(LivingEntity.CreateDummy())
    {
    }

    public Spell(LivingEntity caller)
    {
      this.Caller = caller;
      manaCost = BaseManaCost;
      levelToMagic[1] = 10;
    }

    protected float damage;
    float damageMultiplicator = 45.0f;//%

    //Returns damage based on Spell level.
    //Spell level depends on the magic amount owner has.
    //For enemies magic amount is increased automatically as other stats.
    public float Damage
    {
      get
      {
        var level = GetCurrentLevel();
        var dmg = CalcDamage(level);
        return dmg;
      }
    }

    protected virtual float CalcDamage(int magicLevel)
    {
      var dmg = damage + (damage * ((magicLevel - 1) * (damageMultiplicator + magicLevel * magicLevel / 2) / 100.0f));
      return (float)Math.Ceiling(dmg);
    }

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



    public int GetNextLevelMagicNeeded()
    {
      var index = GetNextLevelMagicIndex();
      return index < 0 ? index : levelToMagic[index];
    }
    public int GetNextLevelMagicIndex()
    {
      var ownerMagicAmount = Caller.Stats.GetCurrentValue(EntityStatKind.Magic);
      while (ownerMagicAmount >= levelToMagic.Values.Last())
      {
        var value = levelToMagic.Values.Last();
        levelToMagic.Add(levelToMagic.Keys.Last() + 1, (int)(value * 1.2));
      }
      var nextValue = levelToMagic.Values.Where(i => i > ownerMagicAmount).First();
      var index = levelToMagic.Where(i => i.Value == nextValue).Single().Key;
      return index;
    }

    public virtual string GetLifetimeSound() { return ""; }
    public virtual string GetHitSound() { return ""; }

    public string[] GetFeatures()
    {
      var fe = new List<string>();
      AppendBasePart(fe);
      AppendPrivateFeatures(fe);
      AppendNextLevel(fe);
      return fe.ToArray();
    }



    //public Tuple<LivingEntity.EffectType, int> GetEffectType()
    //{
    //  var et = new Tuple<LivingEntity.EffectType, int>(LivingEntity.EffectType.None, 0);

    //  switch (Kind)
    //  {
    //    case SpellKind.FireBall:
    //    case SpellKind.NESWFireBall:
    //      et = new Tuple<LivingEntity.EffectType, int>(LivingEntity.EffectType.Firing, 3);
    //      break;
    //    case SpellKind.CrackedStone:
    //      break;
    //    case SpellKind.Skeleton:
    //      break;
    //    case SpellKind.Trap:
    //      break;
    //    case SpellKind.IceBall:
    //      et = new Tuple<LivingEntity.EffectType, int>(LivingEntity.EffectType.Frozen, 3);
    //      break;
    //    case SpellKind.PoisonBall:
    //      et = new Tuple<LivingEntity.EffectType, int>(LivingEntity.EffectType.Poisoned, 3);
    //      break;
    //    //case SpellKind.StonedBall:
    //    //  et = new Tuple<LivingEntity.EffectType, int>(LivingEntity.EffectType.Poisoned, 3);
    //    //  break;
    //    case SpellKind.Transform:
    //      break;
    //    default:
    //      break;
    //  }

    //  return et;
    //}

    public int GetCurrentLevel()
    {
      var lev = GetNextLevelMagicIndex() - 1;
      return lev;
    }

    protected virtual void AppendPrivateFeatures(List<string> fe)
    {
    }

    protected virtual void AppendNextLevel(List<string> fe)
    {
      fe.Add("Next Level: Magic " + GetNextLevelMagicNeeded());
    }

    protected void AppendBasePart(List<string> fe)
    {
      fe.Add("Mana Cost: " + ManaCost);
    }
  }

  public class DefensiveSpell : Spell
  {
    protected Tile tile;
    protected void SetHealthFromLevel(LivingEntity spellTarget, float factor = 1)
    {
      var lvl = GetCurrentLevel();
      var he = GetHealthFromLevel(lvl) * factor;
      spellTarget.Stats.SetNominal(EntityStatKind.Health, he);
    }

    const int baseHealth = 20;
    protected int GetHealthFromLevel(int lvl)
    {
      return FactorCalculator.CalcFromLevel(lvl, baseHealth);
    }

    public virtual Tile Tile
    {
      get
      {
        return tile;
      }

      set
      {
        tile = value;
      }
    }

    protected int CalcTourLasting(float factor = 1)
    {
      return CalcTourLasting(GetCurrentLevel(), factor);
    }

    protected int CalcTourLasting(int magicLevel, float factor = 1)
    {
      var he = GetHealthFromLevel(magicLevel);
      float baseVal = ((float)he) * factor;
      return (int)(baseVal / 4f);
    }

    public DefensiveSpell() : this(LivingEntity.CreateDummy()) { }
    public DefensiveSpell(LivingEntity caller) : base(caller)
    {
    }
  }
}
