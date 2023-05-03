using Roguelike.Spells;
//using UnityEngine;

namespace Roguelike.Abstract.Spells
{
  public interface IStaticSpellFactory
  {
    System.Object CreateSpell(Dungeons.Core.Vector2D pos, SpellKind sk);
  }
}
