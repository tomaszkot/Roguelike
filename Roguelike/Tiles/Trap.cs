//using Dungeons.Core;
//using Dungeons.Tiles;
//using Roguelike.Effects;
//using Roguelike.Spells;
//using Roguelike.Tiles.LivingEntities;
//using System;
//using System.Drawing;

//namespace Roguelike.Tiles
//{
//  public class Trap : Tile
//  {
//    TrapSpell spell;
//    LivingEntity victim;
//    public event EventHandler<Trap> Died;
//    public const char TrapSymbol = '^';
//    public bool SetUp { get; set; }
//    public const string Guid = "53E344A0-45A7-41BB-BF6B-7969709FDE5B";

//    public TrapSpell Spell
//    {
//      get
//      {
//        return spell;
//      }

//      set
//      {
//        spell = value;
//      }
//    }

//    public LivingEntity Victim
//    {
//      get
//      {
//        return victim;
//      }

//      set
//      {
//        victim = value;
//      }
//    }

//    public float BleedingDamage
//    {
//      get
//      {
//        if (Spell != null)
//          return Spell.Damage;
//        return 0;// GetDamage();
//      }
//    }

//    public Trap(Point point, bool setup) 
//    {
//      SetUp = setup;
//      this.point = point;
//      this.Symbol = TrapSymbol;
//      Name = "Trap";

//      tag1 = "trap_animated";
//    }

//    public Trap() : this(new Point().Invalid(), false)
//    {

//    }

//    //protected override float GetAuxFactor(float fac)
//    //{
//    //  return fac;
//    //}

//    public void OnEffectStarted(EffectType type) { }
//    public void OnEffectFinished(EffectType type)
//    {
//      //Victim.ReleaseTrap();
//      //GameManager.Instance.PlaySound("trap_off");

//      if (Died != null)
//        Died(this, this);

//    }

//    public float GetRadius()
//    {
//      //var radius = GetAuxValue(GetAbility().Level);
//      //return radius;
//      return 0;
//    }
//  }
//}
