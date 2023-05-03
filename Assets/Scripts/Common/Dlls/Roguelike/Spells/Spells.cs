using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;

namespace Roguelike.Spells
{
  ///////////////////////////////////////////////////////////////////////////
  public class FireBallSpell : ProjectiveSpell
  {
    public FireBallSpell(LivingEntity caller, Weapon weapon = null) : base(caller, SpellKind.FireBall, weapon)
    {
      manaCost = BaseManaCost * 2;

    }
    public override string GetLifetimeSound() { return "fireball"; }
    public override string GetHitSound() { return "fireball_hit"; }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class LightingBallSpell : ProjectiveSpell
  {
    //public LightingBallSpell() : this(new LivingEntity(), null)
    //{ }
    public LightingBallSpell(LivingEntity caller, Weapon weapon = null) : base(caller, SpellKind.LightingBall, weapon)
    {
      manaCost = BaseManaCost * 2;
    }
    public override string GetLifetimeSound() { return "LightingBallFly"; }
    public override string GetHitSound() { return "LightingBallHit"; }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class NESWFireBallSpell : ProjectiveSpell
  {
    //public NESWFireBallSpell() : this(new LivingEntity(), null)
    //{ }
    public NESWFireBallSpell(LivingEntity caller, Weapon weapon) : base(caller, SpellKind.NESWFireBall, weapon)
    {
      manaCost = BaseManaCost * 4;
      EnemyRequired = false;
      EntityRequired = false;
    }
    public override string GetLifetimeSound() { return "fireball"; }
    public override string GetHitSound() { return "fireball_hit"; }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class IceBallSpell : ProjectiveSpell
  {
    //public IceBallSpell() : this(new LivingEntity(), null)
    //{ }
    public IceBallSpell(LivingEntity caller, Weapon weapon = null) : base(caller, SpellKind.IceBall,weapon)
    {
      manaCost = (BaseManaCost * 2) + 1;
      
    }
    public override string GetLifetimeSound() { return "ice_spell1"; }
    public override string GetHitSound() { return "hit_by_wind"; }

  }

  ///////////////////////////////////////////////////////////////////////////
  public class PoisonBallSpell : ProjectiveSpell
  {
    //public PoisonBallSpell() : this(new LivingEntity(), null)
    //{ }
    public PoisonBallSpell(LivingEntity caller, Weapon weapon = null) : base(caller, SpellKind.PoisonBall,weapon)
    {
      manaCost = BaseManaCost * 2;
      

    }
    public override string GetLifetimeSound() { return "spell"; }
    public override string GetHitSound() { return "gas1"; }
  }

  public class SwiatowitSpell : OffensiveSpell
  {
    public SwiatowitSpell(LivingEntity caller, Weapon weapon = null) : base(caller, SpellKind.Swiatowit, weapon)
    {
      manaCost = BaseManaCost * 2;
    }
    public override string GetLifetimeSound() { return "spell"; }
    public override string GetHitSound() { return "gas1"; }

  }

  public class PerunSpell : OffensiveSpell
  {
    public PerunSpell() : this(new LivingEntity()) { }

    public PerunSpell(LivingEntity caller, Weapon wpn = null) : base(caller, SpellKind.Perun, wpn)
    {
      manaCost = BaseManaCost * 2;
    }

    public override string GetLifetimeSound() { return "axe_swing"; }
    public override string GetHitSound() { return "axe_hit"; }

    public override string HitSound => GetHitSound();
  }

  /// <summary>
  /// 
  /// </summary>
  public class FireStoneSpell : ProjectiveSpell
  {
    public FireStoneSpell(LivingEntity caller, Weapon weapon = null) : base(caller, SpellKind.FireStone, weapon)
    {
      manaCost = BaseManaCost * 2;
    }
    //public override string GetLifetimeSound() { return "spell"; }
    public override string GetHitSound() { return "stone_roll3"; }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class StonedBallSpell : ProjectiveSpell
  {
    //public StonedBallSpell() : this(new LivingEntity())
    //{ }
    public StonedBallSpell(LivingEntity caller) : base(caller, SpellKind.StonedBall, null)
    {
      manaCost = BaseManaCost * 2;
      //damage = ProjectiveSpell.BaseDamage * 3f;
    }
    public override string GetLifetimeSound() { return "thunder3"; }
    //public override string GetHitSound() { return "stone1.wav"; }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class CrackedStoneSpell : PassiveSpell
  {
    public CrackedStoneSpell() : this(new LivingEntity())
    { }

    public CrackedStoneSpell(LivingEntity caller) : base(caller, SpellKind.CrackedStone, EntityStatKind.Unset)
    {
      Tile = new Tiles.Interactive.CrackedStone(caller.Container);
      RequiresDestPoint = true;
      Duration = 0;
    }
  }

  /////////////////////////////////////////////////////////////////////////////
  //public class TrapSpell : OffensiveSpell
  //{
  //  Trap trap = new Trap();

  //  public TrapSpell() : this(new LivingEntity())
  //  { }

  //  public TrapSpell(LivingEntity caller) : base(caller, null)
  //  {
  //    Kind = SpellKind.Trap;

  //    trap.Spell = this;
  //    trap.SetUp = true;

  //    //damage = ProjectiveSpell.BaseDamage * 5f;
  //    manaCost = (float)(BaseManaCost * 2);
  //  }

  //  public Trap Trap { get { return trap; } }
  //}

  ///////////////////////////////////////////////////////////////////////////
  public class BushTrapSpell : OffensiveSpell
  {
    public BushTrapSpell() : this(new LivingEntity())
    { }

    public BushTrapSpell(LivingEntity caller) : base(caller, SpellKind.BushTrap, null)
    {
      
      //damage = ProjectiveSpell.BaseDamage;
      manaCost = (float)(BaseManaCost * 2);
      EnemyRequired = true;
    }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class SkeletonSpell : OffensiveSpell
  {
    Ally enemy;
    Ally enemyNextLevel;

    public Ally Ally { get => enemy; set => enemy = value; }
    public Ally AllyNextLevel { get => enemyNextLevel; set => enemyNextLevel = value; }

    public const int SkeletonSpellStrengthIncrease = 2;
    public const int SkeletonSpellDefenseIncrease = 4;

    public SkeletonSpell() : this(new LivingEntity(), Difficulty.Normal)
    {   
    }

    public SkeletonSpell(LivingEntity caller, Difficulty? diff = null) : base(caller, SpellKind.Skeleton, null)
    {
      var level = CurrentLevel;
      Ally = CreateAlly(caller, diff, level);
      AllyNextLevel = CreateAlly(caller, diff, level+1);

      manaCost = (float)(BaseManaCost * 2) + 2;

      if (caller is AdvancedLivingEntity ale)
      {
        //var ab = ale.GetPassiveAbility(Abilities.AbilityKind.SkeletonMastering);
        //if(ab.Level > 0)
        //  Ally.IncreaseStats(ab);
      }
    }

    private Ally CreateAlly(LivingEntity caller, Difficulty? diff, int level)
    {
      var ally = caller.Container.GetInstance<AlliedEnemy>();
      ally.InitSpawned(EnemySymbols.SkeletonSymbol, level, diff);
      //ally.Stats.SetNominal(EntityStatKind.Dexterity, LivingEntity.StartStatValues[EntityStatKind.Dexterity]+5);
      ally.RecalculateStatFactors(false);
      ally.Name = "Skeleton";
      return ally;
    }

    public override SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel)
    {
      var desc = base.CreateSpellStatsDescription(currentMagicLevel);
      float dmg = 0;
      if (currentMagicLevel)
      {
        dmg = Ally.Stats.MeleeAttack;
      }
      else
        dmg = AllyNextLevel.Stats.MeleeAttack;

      desc.Damage = Ally.GetForDisplay(true, dmg);
      return desc;
    }
  }

  public class TransformSpell : PassiveSpell
  {
    //const float factor = 1.25f;
    public TransformSpell() : this(new LivingEntity())
    { }

    public TransformSpell(LivingEntity caller) : base(caller, SpellKind.Transform, EntityStatKind.Unset)
    {
      CoolingDown = 10;
      Duration--;
    }
  }

  public class DziewannaSpell : PassiveSpell
  {
    public DziewannaSpell() : this(new LivingEntity()){ }

    public DziewannaSpell(LivingEntity caller) : base(caller, SpellKind.Dziewanna, EntityStatKind.Unset)
    {
      CoolingDown = 10;
      StatKind = EntityStatKind.Unset;
      manaCost += 5;
      Duration = 0;
      RequiresDestPoint = true;
    }
  }



  public class SwarogSpell : PassiveSpell
  {
    public SwarogSpell() : this(new LivingEntity()) { }

    public SwarogSpell(LivingEntity caller) : base(caller, SpellKind.Swarog, EntityStatKind.Unset)
    {
      CoolingDown = 10;
      StatKind = EntityStatKind.Unset;
      manaCost += 5;
      Duration = 0;
    }
  }

  public class TeleportSpell : PassiveSpell
  {
    
    public int Range;

    public TeleportSpell() : this(new LivingEntity())
    {
      
    }

    public TeleportSpell(LivingEntity caller) : base(caller, SpellKind.Teleport, EntityStatKind.Unset)
    {
      RequiresDestPoint = true;
      Range = CalcRange(true);
      
      EntityRequired = true;
      CoolingDown = 8;
      Duration = 0;
    }

    

    protected override int GetRange()
    {
      return Range;
    }
  }

  //public class FrightenSpell : DefensiveSpell
  //{
  //  public int TourLasting { get; set; }
  //  public int Range { get; set; }
  //  public FrightenSpell() : this(new LivingEntity())
  //  { }
  //  public FrightenSpell(LivingEntity caller) : base(caller)
  //  {
  //    Kind = SpellKind.Frighten;
  //    TourLasting = CalcTourLasting();
  //    Range = 2;//TODO
  //  }
  //  protected override void AppendPrivateFeatures(List<string> fe)
  //  {
  //    fe.Add("our Lasting: " + TourLasting);
  //  }

  //  protected override void AppendNextLevel(List<string> fe)
  //  {
  //    base.AppendNextLevel(fe);
  //    fe.Add("Next Level: Tour Lasting: " + CalcTourLasting(GetCurrentLevel() + 1));
  //  }
  //}
  
  public class ManaShieldSpell : PassiveSpell
  {
    public ManaShieldSpell() : this(new LivingEntity())
    { }

    public ManaShieldSpell(LivingEntity caller) : base(caller, SpellKind.ManaShield, EntityStatKind.Mana)
    {
    }
  }
  public class SwapPositionSpell : PassiveSpell
  {
    const int baseRange = 3;
    public int Range = baseRange;
    public SwapPositionSpell() : this(new LivingEntity())
    { }

    public SwapPositionSpell(LivingEntity caller) : base(caller, SpellKind.SwapPosition, EntityStatKind.Unset)
    { 
      Duration = 0;
      EntityRequired = true;
      RequiresDestPoint = true;
      Range = CalcRange(true);
    }

    protected override int GetRange()
    {
      return Range;
    }
  }


  //#if UNITY_WSA_10_0
  //#elif UNITY_WSA
  //#else
  //  [Serializable]
  //#endif
  //  public class TelekinesisSpell : DefensiveSpell
  //  {
  //    public TelekinesisSpell() : this(new LivingEntity())
  //    { }
  //    public TelekinesisSpell(LivingEntity caller) : base(caller)
  //    {
  //      EntityRequired = true;
  //      Kind = SpellKind.Telekinesis;
  //    }

  //    protected override void AppendNextLevel(List<string> fe)
  //    {
  //      //fe.Add("Next Level: Magic " + GetNextLevelMagicNeeded());
  //    }
  //  }

  ///////////////////////////////////////////////////////////////////////////

  /////////////////////////////////////////////////////////////////////
  public class ResistAllSpell : PassiveSpell
  {
    public ResistAllSpell() : this(new LivingEntity())
    {
    }

    public ResistAllSpell(LivingEntity caller) : base(caller, SpellKind.ResistAll, EntityStatKind.Unset, 25)
    {
      StatKindPercentage = new Factors.PercentageFactor(0);
      StatKindEffective = new Factors.EffectiveFactor(25);
    }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class WeakenSpell : PassiveSpell
  {
    public WeakenSpell() : this(new LivingEntity())
    {
    }

    public WeakenSpell(LivingEntity caller) : base(caller, SpellKind.Weaken, EntityStatKind.Defense)
    {
      EntityRequired = true;
    }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class InaccuracySpell : PassiveSpell
  {
    public InaccuracySpell() : this(new LivingEntity())
    {
    }

    //TODO EntityStatKind.ChanceToPhysicalProjectileHit es
    public InaccuracySpell(LivingEntity caller) : base(caller, SpellKind.Inaccuracy, EntityStatKind.ChanceToMeleeHit, 15)
    {
      EntityRequired = true;
    }
  }

  /////////////////////////////////////////////////////////////////////////
  public class IronSkinSpell : PassiveSpell
  {
    public IronSkinSpell() : this(new LivingEntity())
    {
    }

    public IronSkinSpell(LivingEntity caller) : base(caller, SpellKind.IronSkin, EntityStatKind.Defense)
    {
      EntityRequired = false;
    }
  }
}
