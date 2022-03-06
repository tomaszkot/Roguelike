using Newtonsoft.Json;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
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
    //Rage = 16, 
    Weaken = 17, NESWFireBall = 18, Teleport = 19, 
    //IronSkin = 20, 
    ResistAll = 25, 
    Inaccuracy = 29, /*CallMerchant, CallGod,*/ Identify = 30, Portal = 33,
    Dziewanna = 40
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
    private LivingEntity caller;
    public bool Utylized { get; set; }

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

    public int CurrentLevel
    {
      get
      {
        if (weaponSpellSource != null)
          return weaponSpellSource.LevelIndex;
        var lev = GetNextLevelMagicIndex() - 1;
        return lev;
      }
    }

    public bool IsFromMagicalWeapon { get => weaponSpellSource != null;}

    //TODO ! remove withVariation, use AttackDesc is needed
    public virtual SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel) 
    {
      int level = currentMagicLevel ? CurrentLevel : CurrentLevel + 1;
      int? mana = null;
      int? magicRequired = null;
      int range = 0;
      if (this is IProjectileSpell proj)
        range = proj.Range;
      if (weaponSpellSource == null)
      {
        mana = CalcManaCost(level);
        magicRequired = NextLevelMagicNeeded;
      }
      var desc = new SpellStatsDescription(level, mana, magicRequired, Kind, range);
      return desc;
    }
  }


}
