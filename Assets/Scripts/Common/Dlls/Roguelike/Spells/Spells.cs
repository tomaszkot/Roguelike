using Roguelike.Abilities;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;

namespace Roguelike.Spells
{


  ///////////////////////////////////////////////////////////////////////////
  public class CrackedStoneSpell : PassiveSpell
  {
    Tiles.Interactive.CrackedStone typedTile;

    public CrackedStoneSpell() : this(new LivingEntity())
    { }

    public CrackedStoneSpell(LivingEntity caller) : base(caller, SpellKind.CrackedStone, EntityStatKind.Unset)
    {
      typedTile = new Tiles.Interactive.CrackedStone(caller.Container);
      typedTile.Durability = this.Durability;
      Tile = typedTile;
      RequiresDestPoint = true;
      UnsetProp(AbilityProperty.Duration);
      BaseRange = 0;
      CalcProp(AbilityProperty.Range, true);
    }

    public CrackedStone TypedTile { get => typedTile; set => typedTile = value; }

    public override SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel)
    {
      var desc = base.CreateSpellStatsDescription(currentMagicLevel);
      if (currentMagicLevel)
      {
        desc.Durability = typedTile.Durability;
      }
      else
        desc.Durability = CalcDurability(CurrentLevel + 1);
      return desc;
    }
  }
    
  public class TransformSpell : PassiveSpell
  {
    public TransformSpell() : this(new LivingEntity())
    { }

    public TransformSpell(LivingEntity caller) : base(caller, SpellKind.Transform, EntityStatKind.Unset)
    {
      CoolingDownCounter = 10;
      BaseDuration--;
      UnsetProp(Abilities.AbilityProperty.Range);
      CalcProp(AbilityProperty.Duration, true);
    }
  }

  public class TeleportSpell : PassiveSpell
  {
    public TeleportSpell() : this(new LivingEntity())
    {
      
    }

    public TeleportSpell(LivingEntity caller) : base(caller, SpellKind.Teleport, EntityStatKind.Unset)
    {
      RequiresDestPoint = true;
      EntityRequired = true;
      CoolingDownCounter = 8;
      UnsetProp(AbilityProperty.Duration);
    }

  }

  public class FrightenSpell : PassiveSpell
  {
    public FrightenSpell() : this(new LivingEntity())
    { }
    public FrightenSpell(LivingEntity caller) : base(caller, SpellKind.Frighten, EntityStatKind.Unset)
    {
    }
    protected override int CalcDuration(int level)
    {
      return base.CalcDuration(level) -2;
    }
  }

  public class ManaShieldSpell : PassiveSpell
  {
    public ManaShieldSpell() : this(new LivingEntity())
    { }

    public ManaShieldSpell(LivingEntity caller) 
      : base(caller, SpellKind.ManaShield, EntityStatKind.Mana)
    {
      UnsetProp(Abilities.AbilityProperty.Range);
    }
  }
  public class SwapPositionSpell : PassiveSpell
  {
   public SwapPositionSpell() : this(new LivingEntity())
    { }

    public SwapPositionSpell(LivingEntity caller) : base(caller, SpellKind.SwapPosition, EntityStatKind.Unset)
    {
      UnsetProp(AbilityProperty.Duration);
      EntityRequired = true;
      RequiresDestPoint = true;
    }
  }

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

  
  /// <summary>
  /// Used only by enemies
  /// </summary>
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
