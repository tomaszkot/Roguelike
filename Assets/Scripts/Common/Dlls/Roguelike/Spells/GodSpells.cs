using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;

namespace Roguelike.Spells
{
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
  public class DziewannaSpell : PassiveSpell
  {
    public DziewannaSpell() : this(new LivingEntity()) { }

    public DziewannaSpell(LivingEntity caller) : base(caller, SpellKind.Dziewanna, EntityStatKind.Unset)
    {
      CoolingDownCounter = 10;
      StatKind = EntityStatKind.Unset;
      manaCost += 5;
      RequiresDestPoint = true;
      UnsetProp(AbilityProperty.Duration);
    }
  }

  public class JarowitSpell : PassiveSpell
  {
    public JarowitSpell() : this(new LivingEntity()) { }

    public JarowitSpell(LivingEntity caller) : base(caller, SpellKind.Jarowit, EntityStatKind.Unset)
    {
      CoolingDownCounter = 10;
      StatKind = EntityStatKind.Unset;
      manaCost += 5;
      RequiresDestPoint = false;
      //UnsetProp(AbilityProperty.Duration);
    }
  }

  public class WalesSpell : PassiveSpell
  {
    public WalesSpell() : this(new LivingEntity()) { }

    public WalesSpell(LivingEntity caller) : base(caller, SpellKind.Wales, EntityStatKind.Unset)
    {
      CoolingDownCounter = 10;
      StatKind = EntityStatKind.Unset;
      manaCost -= 5;
      if (manaCost < 0)
        manaCost = 5;
      RequiresDestPoint = false;
      //UnsetProp(AbilityProperty.Duration);
    }
  }

  public class SwarogSpell : PassiveSpell
  {
    public SwarogSpell() : this(new LivingEntity()) { }

    public SwarogSpell(LivingEntity caller) : base(caller, SpellKind.Swarog, EntityStatKind.Unset)
    {
      CoolingDownCounter = 10;
      StatKind = EntityStatKind.Unset;
      manaCost += 5;
      UnsetProp(AbilityProperty.Duration);
    }
  }

}
