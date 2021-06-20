using Roguelike.Abstract.Spells;
using Roguelike.Spells;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using System;

namespace Roguelike.Tiles.Looting
{
  public class Scroll : SpellSource
  {
    //public bool EnemyRequired { get { return spell.EnemyRequired; } }
    //public bool EntityRequired { get { return spell.EntityRequired; } }
    //Spell spell;

    //public int Level
    //{
    //  get { return spell.GetCurrentLevel(); }
    //}

    public Scroll() : this(SpellKind.Unset)
    {

    }

    public Scroll(SpellKind kind = SpellKind.Unset) : base(kind)
    {
      //dummy.Stats.SetNominal(EntityStatKind.Magic, LivingEntity.BaseMagic.TotalValue);
      LootKind = LootKind.Scroll;
    }

    public static SpellKind DiscoverKindFromName(string name)//->name fire_ball -> FireBall
    {
      return DiscoverKindFromName(name, false);
    }
  }
}
