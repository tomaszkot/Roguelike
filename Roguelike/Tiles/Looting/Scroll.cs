using Roguelike.Abstract.Spells;
using Roguelike.Spells;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using System;

namespace Roguelike.Tiles.Looting
{
  public class Scroll : SpellSource
  {
    public Scroll() : this(SpellKind.Unset){}

    public Scroll(SpellKind kind = SpellKind.Unset) : base(kind)
    {
      LootKind = LootKind.Scroll;
      Price = 5;
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
}
