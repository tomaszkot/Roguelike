using Dungeons.Tiles;
using SimpleInjector;

namespace Roguelike.Generators.TileContainers
{
  //a single room, can have size like 10x10, used only at the time of generation, then copied to the destination container
  public class DungeonNode : Dungeons.DungeonNode
  {
    public DungeonNode(Container c) : base(c)
    { }
    //public DungeonNode(int width = 10, int height = 10, GenerationInfo gi = null,
    //                  int nodeIndex = Dungeons.DungeonNode.DefaultNodeIndex, Generators.TileContainers.DungeonNode parent = null)
    //: base(width, height, gi, nodeIndex, parent)
    //{

    //}

    //public override Dungeons.DungeonNode CreateChildIslandInstance(int w, int h, GenerationInfo gi, Dungeons.DungeonNode parent)
    //{
    //  //TODO use container
    //  return new DungeonNode(w, h, gi, parent: this);
    //}

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
