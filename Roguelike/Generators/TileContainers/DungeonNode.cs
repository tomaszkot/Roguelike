using Dungeons.Tiles;
using SimpleInjector;

namespace Roguelike.Generators.TileContainers
{
  //a single room, can have size like 10x10, used only at the time of generation, then copied to the destination container
  public class DungeonNode : Dungeons.TileContainers.DungeonNode
  {
    public DungeonNode(Container c) : base(c)
    { }

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
