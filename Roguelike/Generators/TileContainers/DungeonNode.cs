using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dungeons;
using Dungeons.Tiles;
using Roguelike.Tiles;

namespace Roguelike.Generators.TileContainers
{
  //a single room, can have size like 10x10, used only at the time of generation, then copied to the destination container
  public class DungeonNode : Dungeons.DungeonNode
  {
    

    public DungeonNode(int width = 10, int height = 10, GenerationInfo gi = null,
                      int nodeIndex = Dungeons.DungeonNode.DefaultNodeIndex, Generators.TileContainers.DungeonNode parent = null)
    : base(width, height, gi, nodeIndex, parent)
    {

    }

    protected override Dungeons.Tiles.Door CreateDoorInstance()
    {
      return new Tiles.Door();
    }

    protected override bool ShallReveal(int row, int col)
    {
      var reveal = tiles[row, col].DungeonNodeIndex == NodeIndex; 
      if (!reveal)
      {
        if (tiles[row, col].IsFromChildIsland)
        {
          //if (tiles[row, col] is Tiles.Door)
          //{
          //  int k = 0;
          //}
          reveal = (tiles[row, col] is Wall) && (tiles[row, col] as Wall).IsSide || (tiles[row, col] is Dungeons.Tiles.Door);
        }
      }

      return reveal;
    }
  }
}
