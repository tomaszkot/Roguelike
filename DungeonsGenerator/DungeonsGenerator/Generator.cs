using Dungeons.Core;
using Dungeons.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons
{
  public interface IDungeonGenerator
  {
    DungeonNode Generate(Container container, int levelIndex);
  }

  public class DungeonGenerator : IDungeonGenerator
  {
    static protected Random random;
    protected List<DungeonNode> nodes;
    protected Container container;
    //int levelCounter;

    static DungeonGenerator()
    {
      random = new Random();
    }

    public virtual DungeonNode Generate(Container container,int levelIndex)
    {
      this.container = container;
      return Generate<DungeonNode>(levelIndex);
    }

    Tile GetPossibleDoorTile(List<Tile> listOne, List<Tile> listTwo)
    {
      var common = listOne.SelectMany(x => listTwo.Where(y => y.IsAtSamePosition(x))).ToList();
      int doorIndex = random.Next(common.Count);
      if (doorIndex == 0)
        doorIndex++;
      if (doorIndex == common.Count - 1)
        doorIndex--;
      return common[doorIndex];
    }

    DungeonNode CreateNode(int nodeIndex)
    {
      GenerationInfo gi = CreateNodeGenerationInfo();
      return CreateNode(nodeIndex, gi);
    }

    protected virtual DungeonNode CreateNode(int nodeIndex, GenerationInfo gi)
    {
      var minNodeSize = nodeIndex == 0 && gi.FirstNodeSmaller ? gi.MinNodeSize - gi.MinNodeSize / 2 : gi.MinNodeSize;
      var maxNodeSize = nodeIndex == 0 && gi.FirstNodeSmaller ? gi.MaxNodeSize - gi.MaxNodeSize / 2 : gi.MaxNodeSize;

      var width = random.Next(minNodeSize, maxNodeSize);
      var height = random.Next(minNodeSize, maxNodeSize);

      return CreateNode(width, height , gi, nodeIndex);
    }

    protected virtual DungeonNode CreateNode(int w, int h, GenerationInfo gi, int nodeIndex)
    {
      //TODO use container
      return new DungeonNode(w, h, gi, nodeIndex);
    }
    protected virtual DungeonNode CreateLevel(int levelIndex, int w, int h, GenerationInfo gi)
    {
      //TODO use container
      return new DungeonNode(w, h, gi);
    }


    public virtual int NumberOfNodes
    {
      get
      {
        return GenerationInfo.NumberOfNodes;
      }
    }
    
    //TODO public
    public virtual List<DungeonNode> CreateDungeonNodes()
    {
      nodes = new List<DungeonNode>();
      var gi = this.CreateLevelGenerationInfo();
      //gi.GenerateOuterWalls = true;
      //for (int i = 0; i < NumberOfNodes; i++)
      for (int i = 0; i < GenerationInfo.NumberOfNodes; i++)
      {
        var node = CreateNode(i, gi);
        nodes.Add(node);
      }
      return nodes;
    }

    protected virtual GenerationInfo CreateNodeGenerationInfo()
    {
      return new GenerationInfo();
    }



    protected virtual GenerationInfo CreateLevelGenerationInfo()
    {
      var gi = new GenerationInfo();
      //gi.GenerateOuterWalls = false;

      return gi;
    }

    /// <summary>
    /// Generates a dungeon 
    /// </summary>
    /// <param name="mazeNodes"></param>
    /// <returns></returns>
    protected virtual T Generate<T>(int levelIndex, LayouterOptions opt = null) where T : DungeonNode, new()
    {
      var mazeNodes = CreateDungeonNodes();
      
      var layouter = new DefaultNodeLayouter();
      T level = layouter.DoLayout<T>(mazeNodes, opt);

      return level;
    }
  }
}
