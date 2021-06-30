using Newtonsoft.Json;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
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
    Unset, FireBall, CrackedStone, Skeleton, Trap, IceBall, PoisonBall, Transform,
    Frighten, Healing, ManaShield, Telekinesis, StonedBall, LightingBall
        //,MindControl
        , Mana, BushTrap, Rage, Weaken, NESWFireBall, Teleport, IronSkin, ResistAll, Inaccuracy, /*CallMerchant, CallGod,*/ Identify, Portal
  }

  public class Spell : ISpell
  {
    protected float manaCost;

    float manaCostMultiplicator = 20;
    public bool SendByGod { get; set; }
    public FightItem FightItem { get; internal set; }

    public SpellKind Kind { get; set; }
    public bool EnemyRequired = false;
    public bool EntityRequired = false;
    public const int BaseManaCost = 4;
    protected Dictionary<int, int> levelToMagic = new Dictionary<int, int>();
    private LivingEntity caller;
    public bool Utylized { get; set; }

    public int ManaCost
    {
      get
      {
        return CalcManaCost(CurrentLevel);
      }
    }

    public Spell() : this(LivingEntity.CreateDummy())
    {
    }

    public Spell(LivingEntity caller)
    {
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
        return index< 0 ? index : levelToMagic[index];
      }
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

    //public string[] GetFeatures(bool currentLevel)
    //{
    //  var fe = new List<string>();
    //  AppendBasePart(fe);
    //  AppendPrivateFeatures(fe);
    //  //AppendNextLevel(fe);
    //  return fe.ToArray();
    //}

    //protected static string GetNextLevel(string suffix)
    //{
    //  return "Next Level: " + suffix;
    //}

    public int CurrentLevel
    {
      get
      {
        var lev = GetNextLevelMagicIndex() - 1;
        return lev;
      }
    }

    //protected virtual void AppendPrivateFeatures(List<string> fe)
    //{
    //}

    //protected virtual void AppendNextLevel(List<string> fe)
    //{
    //  fe.Add("Magic: " + GetNextLevelMagicNeeded());
    //}

    //protected void AppendBasePart(List<string> fe)
    //{
    //  fe.Add("Mana Cost: " + ManaCost);
    //}

    //string[] extraStatDescription = new string[0];

    //public string[] GetLevelDescription(bool currentLevel)
    //{
    //  //string currentLevelDesc = "Current Level: " + GetCurrentLevel();
    //  //string manaCost = "Mana Cost: " + ManaCost;
    //  ////string damage = "Damage: "
    //  //extraStatDescription = new string[2];
    //  //extraStatDescription[0] = currentLevelDesc;
    //  //extraStatDescription[1] = manaCost;
    //  return extraStatDescription;
    //}

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

    public virtual SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel) 
    {
      int level = currentMagicLevel ? CurrentLevel : CurrentLevel + 1;
      var desc = new SpellStatsDescription(level, CalcManaCost(level), NextLevelMagicNeeded, Kind);
      return desc;
    }
  }


}
