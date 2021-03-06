using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Spells
{
  ///////////////////////////////////////////////////////////////////////////
  public class FireBallSpell : ProjectiveSpell
  {
    public FireBallSpell(LivingEntity caller) : base(caller)
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
    public LightingBallSpell() : this(new LivingEntity())
    { }
    public LightingBallSpell(LivingEntity caller) : base(caller)
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
    public NESWFireBallSpell() : this(new LivingEntity())
    { }
    public NESWFireBallSpell(LivingEntity caller) : base(caller)
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
    public IceBallSpell() : this(new LivingEntity())
    { }
    public IceBallSpell(LivingEntity caller) : base(caller)
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
    public PoisonBallSpell() : this(new LivingEntity())
    { }
    public PoisonBallSpell(LivingEntity caller) : base(caller)
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
    public StonedBallSpell() : this(new LivingEntity())
    { }
    public StonedBallSpell(LivingEntity caller) : base(caller)
    {
      manaCost = BaseManaCost * 2;
      Kind = SpellKind.StonedBall;
      damage = ProjectiveSpell.BaseDamage * 3f;
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
      Tile = new Tiles.CrackedStone();
      SetHealthFromLevel(Tile as LivingEntity, 4f);
    }

    protected override void AppendPrivateFeatures(List<string> fe)
    {
      fe.Add("Durability: " + GetHealthFromLevel(GetCurrentLevel()));
    }

    protected override void AppendNextLevel(List<string> fe)
    {
      base.AppendNextLevel(fe);
      fe.Add(GetNextLevel("Durability " + GetHealthFromLevel(GetCurrentLevel() + 1)));
    }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class TrapSpell : OffensiveSpell
  {
    Trap trap = new Trap();

    public TrapSpell() : this(new LivingEntity())
    { }

    public TrapSpell(LivingEntity caller) : base(caller)
    {
      Kind = SpellKind.Trap;

      trap.Spell = this;
      trap.SetUp = true;
      
      damage = ProjectiveSpell.BaseDamage * 5f;
      manaCost = (float)(BaseManaCost * 2);
    }

    public Trap Trap { get { return trap; } }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class BushTrapSpell : OffensiveSpell
  {
    public BushTrapSpell() : this(new LivingEntity())
    { }

    public BushTrapSpell(LivingEntity caller) : base(caller)
    {
      Kind = SpellKind.BushTrap;
      damage = ProjectiveSpell.BaseDamage;
      manaCost = (float)(BaseManaCost * 2);
      EnemyRequired = true;
    }
  }

  ///////////////////////////////////////////////////////////////////////////
  public class SkeletonSpell : OffensiveSpell
  {
    Enemy enemy;

    public Enemy Enemy { get => enemy; set => enemy = value; }

    public SkeletonSpell() : this(new LivingEntity())
    { }

    public SkeletonSpell(LivingEntity caller) : base(caller)
    {
      Kind = SpellKind.Skeleton;
      damage = ProjectiveSpell.BaseDamage + 1;

      var level = GetCurrentLevel();
      Enemy = Enemy.Spawn(EnemySymbols.SkeletonSymbol, level);
      Enemy.HeroAlly = true;
      Enemy.Stats[EntityStatKind.Attack].Nominal = Damage;
      var health = CalcHealth(level);
      Enemy.Stats[EntityStatKind.Health].Nominal = health;
      manaCost = (float)(BaseManaCost * 2) + 2;
    }

    float CalcHealth(int magicLevel)
    {
      var hfl = PassiveSpell.CalcHealthFromLevel(magicLevel);
      var val = (int)(hfl * (3 + 6 * ((float)(magicLevel * 10) / 100f)));

      var inc = val * 80f / 100f;
      inc *= ((float)magicLevel) / 15f;
      return (int)(val + inc);
    }

    protected override float CalcDamage(int magicLevel)
    {
      var baseD = base.CalcDamage(magicLevel);
      baseD += (float)(baseD * 25.0 / 100f);
      return baseD;
    }

    protected override void AppendPrivateFeatures(List<string> fe)
    {
      fe.Add("Health: " + Enemy.Stats.GetNominal(EntityStatKind.Health));
      fe.Add("Attack: " + Enemy.Stats.GetNominal(EntityStatKind.Attack));
    }

    protected override void AppendNextLevel(List<string> fe)
    {
      base.AppendNextLevel(fe);
      fe.Add(GetNextLevel("Health " + CalcHealth(GetCurrentLevel() + 1)));
      fe.Add(GetNextLevel("Attack " + CalcDamage(GetCurrentLevel() + 1)));
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
    }

    protected override void AppendPrivateFeatures(List<string> fe)
    {
      fe.Add(GetCoolingDown());
      fe.Add(GetTourLasting(TourLasting));
    }

    protected override void AppendNextLevel(List<string> fe)
    {
      base.AppendNextLevel(fe);
      fe.Add(GetNextLevelTourLasting());
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
      Range += GetCurrentLevel();
      if (Range < baseRange + 1)
        Range = baseRange + 1;
      EntityRequired = true;
      Kind = SpellKind.Teleport;
      CoolingDown = 8;
    }

    protected override void AppendPrivateFeatures(List<string> fe)
    {
      fe.Add("Range: " + (Range));
      fe.Add(GetCoolingDown());
      fe.Add(GetTourLasting(TourLasting));
    }

    protected override void AppendNextLevel(List<string> fe)
    {
      base.AppendNextLevel(fe);
      fe.Add(GetNextLevel("Range: " + (Range + 1)));
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

  //#if UNITY_WSA_10_0
  //#elif UNITY_WSA
  //#else
  //  [Serializable]
  //#endif
  //  public class DynamicPropSpell : DefensiveSpell
  //  {
  //    public int IncPercentage { get; set; }

  //    public DynamicPropSpell(LivingEntity caller) : base(caller)
  //    {
  //      IncPercentage = CalcIncPercentage(GetCurrentLevel());
  //    }

  //    protected int CalcIncPercentage(int magicLevel)
  //    {
  //      var inc = GetHealthFromLevel(magicLevel);

  //      GameManager.Instance.Assert(inc < 100);
  //      if (inc > 100)
  //        inc = 100;

  //      return inc / 2;
  //    }
  //  }


  //#if UNITY_WSA_10_0
  //#elif UNITY_WSA
  //#else
  //  [Serializable]
  //#endif
  //  public class HealingSpell : DynamicPropSpell
  //  {

  //    public HealingSpell() : this(new LivingEntity())
  //    { }
  //    public HealingSpell(LivingEntity caller) : base(caller)
  //    {
  //      Kind = SpellKind.Healing;
  //    }

  //    protected override void AppendPrivateFeatures(List<string> fe)
  //    {
  //      fe.Add("Health restored: " + IncPercentage + " %");
  //    }

  //    protected override void AppendNextLevel(List<string> fe)
  //    {
  //      base.AppendNextLevel(fe);
  //      fe.Add("Next Level: Health restored: " + CalcIncPercentage(GetCurrentLevel() + 1) + " %");
  //    }

  //    public void Apply()
  //    {
  //      var baseVal = Caller.Stats.Stats[EntityStatKind.Health].TotalValue;
  //      var val = baseVal * ((float)IncPercentage) / 100.0f;
  //      Caller.Stats.IncreaseStatDynamicValue(EntityStatKind.Health, val);
  //    }
  //  }

  //#if UNITY_WSA_10_0
  //#elif UNITY_WSA
  //#else
  //  [Serializable]
  //#endif
  //  public class ManaSpell : DynamicPropSpell
  //  {
  //    public int ManaIncPercentage;
  //    public int HealthDecPercentage;

  //    public ManaSpell() : this(new LivingEntity())
  //    {
  //    }

  //    public ManaSpell(LivingEntity caller) : base(caller)
  //    {
  //      Kind = SpellKind.Mana;
  //      ManaIncPercentage = CalcIncPercentage(GetCurrentLevel());
  //      HealthDecPercentage = CalcHealthDecPercentage(ManaIncPercentage);
  //      manaCost = 0;
  //    }

  //    int CalcHealthDecPercentage(int ManaIncPercentage)
  //    {
  //      return ManaIncPercentage + (int)(ManaIncPercentage * 70f / 100f);
  //    }

  //    protected override void AppendPrivateFeatures(List<string> fe)
  //    {
  //      fe.Add("Health sacerficed: " + HealthDecPercentage + " %");
  //      fe.Add("Mana restored: " + ManaIncPercentage + " %");
  //    }

  //    protected override void AppendNextLevel(List<string> fe)
  //    {
  //      base.AppendNextLevel(fe);
  //      var val = CalcIncPercentage(GetCurrentLevel() + 1);
  //      fe.Add("Next Level: Health sacerficed: " + CalcHealthDecPercentage(val) + " %");
  //      fe.Add("Next Level: Mana restored: " + val + " %");
  //    }

  //    public bool Apply()
  //    {
  //      var health = Caller.Stats.Stats[EntityStatKind.Health].TotalValue;
  //      var healthChange = health * ((float)HealthDecPercentage) / 100.0f;
  //      if (Caller.Stats.GetCurrentValue(EntityStatKind.Health) <= healthChange)
  //        return false;
  //      if (!Caller.Stats.IncreaseStatDynamicValue(EntityStatKind.Health, -healthChange))
  //        return false;

  //      var nominalM = Caller.Stats.Stats[EntityStatKind.Mana].TotalValue;
  //      var manaVal = nominalM * ((float)ManaIncPercentage) / 100.0f;
  //      Caller.Stats.IncreaseStatDynamicValue(EntityStatKind.Mana, manaVal);
  //      return true;
  //    }
  //  }

  public class ManaShieldSpell : PassiveSpell
  {
    //public int TourLasting { get; set; }
    public ManaShieldSpell() : this(new LivingEntity())
    { }

    public ManaShieldSpell(LivingEntity caller) : base(caller, EntityStatKind.Mana)
    {
      Kind = SpellKind.ManaShield;
      //TourLasting = GetHealthFromLevel(GetCurrentLevel()) / 3;
      //manaCost = BaseManaCost * 2;
    }

    //protected override void AppendPrivateFeatures(List<string> fe)
    //{
    //  fe.Add("Tour Lasting: " + TourLasting);
    //}

    //protected override void AppendNextLevel(List<string> fe)
    //{
    //  base.AppendNextLevel(fe);
    //  fe.Add("Next Level: Tour Lasting: " + CalcTourLasting(GetCurrentLevel() + 1));
    //}
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

    public RageSpell(LivingEntity caller) : base(caller, EntityStatKind.Attack)
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

    protected override void AppendPrivateFeatures(List<string> fe)
    {
      fe.Add(Kind.ToDescription() + ":" + StatKindEffective);
      fe.Add(GetTourLasting(TourLasting));
    }

    protected override void AppendNextLevel(List<string> fe)
    {
      base.AppendNextLevel(fe);
      var suffix = Kind.ToDescription() + ":" + CalcFactor(GetCurrentLevel() + 1);
      fe.Add(GetNextLevel(suffix));
      fe.Add(GetNextLevelTourLasting());
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

    public InaccuracySpell(LivingEntity caller) : base(caller, EntityStatKind.ChanceToHit, 15)
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
