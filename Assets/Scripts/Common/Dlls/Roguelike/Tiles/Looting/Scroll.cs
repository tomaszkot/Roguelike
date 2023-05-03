using Dungeons.Core;
using Roguelike.Abstract.Spells;
using Roguelike.Spells;
using Roguelike.Tiles.LivingEntities;
using System;
//using UnityEditor.Build;

namespace Roguelike.Tiles.Looting
{
  public class Scroll : SpellSource
  {
    public Scroll() : this(SpellKind.Unset) { }

    public Scroll(SpellKind kind = SpellKind.Unset) : base(kind)
    {
      LootKind = LootKind.Scroll;
      Price = 5;
      if (GodKind)
        Price *= 2;
    }

    public static SpellKind DiscoverKindFromName(string name)//->name fire_ball -> FireBall
    {
      return DiscoverKindFromName(name, false);
    }
    public override ISpell CreateSpell()
    {
      throw new Exception("Call the one with caller");
    }
  }

  public class SwiatowitScroll : Scroll
  {
    public const int MaxRange = 8;

    public SwiatowitScroll() : base(SpellKind.Swiatowit)
    {

    }

    public override string GetNameFromKind()
    {
      return "Swiatowit Scroll";
    }
    public virtual ISpell CreateNextSpell(LivingEntity caller)
    {
      ProjectiveSpell spell = null;
      var rand = RandHelper.GetRandomDouble();
      if(rand < 0.33f)
        spell = new FireBallSpell(caller);
      else if (rand < 0.66f)
        spell = new IceBallSpell(caller);
      else
        spell = new PoisonBallSpell(caller);

      spell.CurrentLevel = caller.Level;
      //spell.Damage = spell.GetDamage();
      spell.Range = MaxRange;
      return spell;
    }

    public override ISpell CreateSpell(LivingEntity caller)
    {
      return CreateNextSpell(caller);
    }

    public override string GetTypeName()
    {
      return "Scroll";
    }
  }
    
}
