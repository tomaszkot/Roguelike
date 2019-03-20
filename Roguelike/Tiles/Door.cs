using Dungeons.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public class Door : Dungeons.Tiles.Door
  {
    public bool Opened { get; set; }

    public Door(Point point) : base(point)
    {
 
    }

    public Door() : this(GenerationConstraints.InvalidPoint)
    {

    }
  }
}
