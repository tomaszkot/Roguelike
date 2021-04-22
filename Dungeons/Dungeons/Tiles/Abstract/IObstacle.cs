using Dungeons.Tiles.Abstract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons.Tiles
{
  public interface IObstacle
  {
    bool OnHitBy(ISpell md);
    Point Position { get; }
  }
}
