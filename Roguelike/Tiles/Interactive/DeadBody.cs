using Dungeons.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Interactive
{
  public class DeadBody : InteractiveTile, ILootSource
  {
    
    public DeadBody(Point point) : base('~')
    {
      Name = "Dead body";
      tag1 = "dead_body";
      DestroySound = "uncloth";
      InteractSound = "uncloth";
    }

    public DeadBody() : this(new Point().Invalid())
    {

    }

    
    public string OriginMap { get; set; }

    public bool SetLevel(int level, Difficulty? diff = null)
    {
      Level = level;

      return true;
    }

    public Point GetPoint()
    {
      return point;
    }
  }
}
