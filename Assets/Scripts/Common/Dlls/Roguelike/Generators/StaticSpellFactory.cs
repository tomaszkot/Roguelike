using Dungeons.Core;
using Roguelike.Abstract.Spells;
using Roguelike.Spells;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roguelike.Generators
{
  public class StaticSpellFactory : IStaticSpellFactory
  {

    public System.Object CreateSpell(Vector2D pos, SpellKind sk)
    {
      return null;
    }
  }
}
