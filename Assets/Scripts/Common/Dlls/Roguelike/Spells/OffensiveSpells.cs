using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Text;

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
    public IceBallSpell(LivingEntity caller, Weapon weapon = null) : base(caller, SpellKind.IceBall, weapon)
    {
      manaCost = (BaseManaCost * 2) + 1;

    }
    public override string GetLifetimeSound() { return "ice_spell1"; }
    public override string GetHitSound() { return "hit_by_wind"; }

  }

  ///////////////////////////////////////////////////////////////////////////
  public class PoisonBallSpell : ProjectiveSpell
  {
    public PoisonBallSpell(LivingEntity caller, Weapon weapon = null) : base(caller, SpellKind.PoisonBall, weapon)
    {
      manaCost = BaseManaCost * 2;


    }
    public override string GetLifetimeSound() { return "spell"; }
    public override string GetHitSound() { return "gas1"; }
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
      UnsetProp(Abilities.AbilityProperty.Range);
      
      Ally = CreateAlly(caller, diff, level);
      AllyNextLevel = CreateAlly(caller, diff, level + 1);

      manaCost = (float)(BaseManaCost * 2) + 2;
      UnsetProp(Abilities.AbilityProperty.Duration);
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
}
