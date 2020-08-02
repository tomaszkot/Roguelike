using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Spells;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public class Trap : Tile//: FightItem, ILastingEffectOwner
  {
    TrapSpell spell;
    LivingEntity victim;
    public event EventHandler<Trap> Died;
    public const char TrapSymbol = '^';
    public bool SetUp { get; set; }
    public const string Guid = "53E344A0-45A7-41BB-BF6B-7969709FDE5B";
    //float radious = 1;

    public TrapSpell Spell
    {
      get
      {
        return spell;
      }

      set
      {
        spell = value;
      }
    }

    public LivingEntity Victim
    {
      get
      {
        return victim;
      }

      set
      {
        victim = value;
      }
    }

    public float BleedingDamage
    {
      get
      {
        if (Spell != null)
          return Spell.Damage;
        return 0;// GetDamage();
      }
    }

    public Trap(Point point, bool setup) //: base(point, TrapSymbol)
    {
      SetUp = setup;
      this.Point = point;
      this.Symbol = TrapSymbol;
      Name = "Trap";

      tag1 = "trap_animated";
      
      //Price = 10;
      //StackedInventoryId = new Guid(Guid);
      //abilityKind = AbilityKind.HuntingMastering;
      //Kind = FightItemKind.Trap;
      //baseDamage = 12;
      //auxFactorName = "Radius";
      //AlwaysCausesEffect = true;
    }

    public Trap() : this(new Point().Invalid(), false)
    {

    }

    //protected override float GetAuxFactor(float fac)
    //{
    //  return fac;
    //}

    public void OnEffectStarted(EffectType type) { }
    public void OnEffectFinished(EffectType type)
    {
      //Victim.ReleaseTrap();
      //GameManager.Instance.PlaySound("trap_off");

      if (Died != null)
        Died(this, this);

    }

    public float GetRadius()
    {
      //var radius = GetAuxValue(GetAbility().Level);
      //return radius;
      return 0;
    }

    //public override float GetAuxValue(int abilityLevel)
    //{
    //  //return this.radious + base.GetAuxValue(GetAbility().Level);
    //  return 0;
    //}

    //public override bool IsPercentage(bool primary)
    //{
    //  return primary ? false : false;
    //}

    //public override string GetPrimaryStatDescription()
    //{
    //  return "Classic hunter's trap, causes bleeding";
    //}
  }
}
