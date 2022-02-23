using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Spells
{
  ///////////////////////////////////////////////////////////////////////////
  public class FireBallSpell : ProjectiveSpell
  {
    public FireBallSpell(LivingEntity caller, Weapon weapon = null) : base(caller, weapon)
    {
      manaCost = BaseManaCost * 2;
      Kind = SpellKind.FireBall;
    }
    public override string GetLifetimeSound() { return "fireball"; }
    public override string GetHitSound() { return "fireball_hit"; }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class LightingBallSpell : ProjectiveSpell
  {
    //public LightingBallSpell() : this(new LivingEntity(), null)
    //{ }
    public LightingBallSpell(LivingEntity caller, Weapon weapon = null) : base(caller, weapon)
    {
      manaCost = BaseManaCost * 2;
      Kind = SpellKind.LightingBall;
    }
    public override string GetLifetimeSound() { return "LightingBallFly"; }
    public override string GetHitSound() { return "LightingBallHit"; }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class NESWFireBallSpell : ProjectiveSpell
  {
    //public NESWFireBallSpell() : this(new LivingEntity(), null)
    //{ }
    public NESWFireBallSpell(LivingEntity caller, Weapon weapon) : base(caller, weapon)
    {
      manaCost = BaseManaCost * 4;
      Kind = SpellKind.NESWFireBall;
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
    public IceBallSpell(LivingEntity caller, Weapon weapon = null) : base(caller, weapon)
    {
      manaCost = (BaseManaCost * 2) + 1;
      Kind = SpellKind.IceBall;
    }
    public override string GetLifetimeSound() { return "ice_spell1"; }
    public override string GetHitSound() { return "hit_by_wind"; }

  }

  ///////////////////////////////////////////////////////////////////////////
  public class PoisonBallSpell : ProjectiveSpell
  {
    //public PoisonBallSpell() : this(new LivingEntity(), null)
    //{ }
    public PoisonBallSpell(LivingEntity caller, Weapon weapon = null) : base(caller, weapon)
    {
      manaCost = BaseManaCost * 2;
      Kind = SpellKind.PoisonBall;

    }
    public override string GetLifetimeSound() { return "spell"; }
    public override string GetHitSound() { return "gas1"; }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class StonedBallSpell : ProjectiveSpell
  {
    //public StonedBallSpell() : this(new LivingEntity())
    //{ }
    public StonedBallSpell(LivingEntity caller) : base(caller, null)
    {
      manaCost = BaseManaCost * 2;
      Kind = SpellKind.StonedBall;
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

    public CrackedStoneSpell(LivingEntity caller) : base(caller, EntityStatKind.Unset)
    {
      Kind = SpellKind.CrackedStone;
      Tile = new Tiles.LivingEntities.CrackedStone(caller.Container);
      SetHealthFromLevel(Tile as LivingEntity, 4f);
    }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class TrapSpell : OffensiveSpell
  {
    Trap trap = new Trap();

    public TrapSpell() : this(new LivingEntity())
    { }

    public TrapSpell(LivingEntity caller) : base(caller, null)
    {
      Kind = SpellKind.Trap;

      trap.Spell = this;
      trap.SetUp = true;

      //damage = ProjectiveSpell.BaseDamage * 5f;
      manaCost = (float)(BaseManaCost * 2);
    }

    public Trap Trap { get { return trap; } }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class BushTrapSpell : OffensiveSpell
  {
    public BushTrapSpell() : this(new LivingEntity())
    { }

    public BushTrapSpell(LivingEntity caller) : base(caller, null)
    {
      Kind = SpellKind.BushTrap;
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

    public const int SkeletonSpellStrengthIncrease = 5;

    public SkeletonSpell() : this(new LivingEntity(), Difficulty.Normal)
    { 
    }

    public SkeletonSpell(LivingEntity caller, Difficulty? diff = null) : base(caller, null)
    {
      Kind = SpellKind.Skeleton;
      var level = CurrentLevel;
      Ally = CreateAlly(caller, diff, level);
      AllyNextLevel = CreateAlly(caller, diff, level+1);

      manaCost = (float)(BaseManaCost * 2) + 2;
    }

    private Ally CreateAlly(LivingEntity caller, Difficulty? diff, int level)
    {
      var ally = caller.Container.GetInstance<AlliedEnemy>();
      ally.InitSpawned(EnemySymbols.SkeletonSymbol, level, diff);
      ally.Stats.SetNominal(EntityStatKind.Strength, AdvancedLivingEntity.BaseStrength.Value.Nominal + SkeletonSpellStrengthIncrease);//same as hero
      ally.Stats.SetNominal(EntityStatKind.Dexterity, AdvancedLivingEntity.BaseDexterity.Value.Nominal + 5);
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

    public TransformSpell(LivingEntity caller) : base(caller, EntityStatKind.Unset)
    {
      Kind = SpellKind.Transform;
      CoolingDown = 10;
      Duration--;
    }
  }

  public class DziewannaSpell : PassiveSpell
  {
    public DziewannaSpell() : this(new LivingEntity()){ }

    public DziewannaSpell(LivingEntity caller) : base(caller, EntityStatKind.Unset)
    {
      Kind = SpellKind.Dziewanna;
      CoolingDown = 10;
      StatKind = EntityStatKind.Health;
    }
  }
  

  public class TeleportSpell : PassiveSpell
  {
    const int baseRange = 3;
    public int Range = baseRange;

    public TeleportSpell() : this(new LivingEntity())
    {

    }

    public TeleportSpell(LivingEntity caller) : base(caller, EntityStatKind.Unset)
    {
      Range += CurrentLevel;
      if (Range < baseRange + 1)
        Range = baseRange + 1;
      EntityRequired = true;
      Kind = SpellKind.Teleport;
      CoolingDown = 8;
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

    public ManaShieldSpell(LivingEntity caller) : base(caller, EntityStatKind.Mana)
    {
      Kind = SpellKind.ManaShield;
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
  public class RageSpell : PassiveSpell
  {
    public RageSpell() : this(new LivingEntity())
    {
    }

    public RageSpell(LivingEntity caller) : base(caller, EntityStatKind.MeleeAttack)
    {
      Kind = SpellKind.Rage;
    }
  }

  /////////////////////////////////////////////////////////////////////
  public class ResistAllSpell : PassiveSpell
  {
    public ResistAllSpell() : this(new LivingEntity())
    {
    }

    public ResistAllSpell(LivingEntity caller) : base(caller, EntityStatKind.Unset, 25)
    {
      Kind = SpellKind.ResistAll;
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

    public WeakenSpell(LivingEntity caller) : base(caller, EntityStatKind.Defense)
    {
      Kind = SpellKind.Weaken;
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
    public InaccuracySpell(LivingEntity caller) : base(caller, EntityStatKind.ChanceToMeleeHit, 15)
    {
      Kind = SpellKind.Weaken;
      //TourLasting = (CalcTourLasting() * 2) / 3;
      EntityRequired = true;
    }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class IronSkinSpell : PassiveSpell
  {
    public IronSkinSpell() : this(new LivingEntity())
    {
    }

    public IronSkinSpell(LivingEntity caller) : base(caller, EntityStatKind.Defense)
    {
      Kind = SpellKind.IronSkin;
      EntityRequired = false;
    }
  }
}
