using Roguelike.Abstract.Spells;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Abstract
{
  public interface IDestroyable : ILootSource
  {
    bool OnHitBy(ISpell md);
    Point Position { get; }
    bool Destroyed { get; set; }
  }
}
