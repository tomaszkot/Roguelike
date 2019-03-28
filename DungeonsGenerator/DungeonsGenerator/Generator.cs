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
    DungeonLevel Generate(int levelIndex, LayouterOptions opt = null);
  }

  //result of composition  of many DungeonNodes 
  public class DungeonLevel : DungeonNode
  {
    public DungeonLevel(Container container): base (container)
    {
      
    }
  }

  public class DungeonGenerator : IDungeonGenerator
  {
    static protected Random random;
    protected List<DungeonNode> nodes;
    protected Container container;

    static DungeonGenerator()
    {
      random = new Random();
    }

    public DungeonGenerator(Container container)
    {
      this.container = container;
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

      return CreateNode(width, height, gi, nodeIndex);
    }

    protected DungeonNode CreateNode(int w, int h, GenerationInfo gi, int nodeIndex)
    {
      var dungeon = container.GetInstance<DungeonNode>();
      dungeon.Create(w, h, gi, nodeIndex);
      return dungeon;
    }

    protected virtual DungeonNode CreateLevel(int levelIndex, int w, int h, GenerationInfo gi)
    {
      var dungeon = container.GetInstance<DungeonNode>();
      dungeon.Create(w, h, gi);
      return dungeon;
    }

    //public virtual int NumberOfNodes
    //{
    //  get
    //  {
    //    return GenerationInfo.NumberOfNodes;
    //  }
    //}

    //TODO public
    public virtual List<DungeonNode> CreateDungeonNodes()
    {
      nodes = new List<DungeonNode>();
      var gi = this.CreateLevelGenerationInfo();
      //gi.GenerateOuterWalls = true;
      //for (int i = 0; i < NumberOfNodes; i++)
      for (int i = 0; i < gi.NumberOfNodes; i++)
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
      return gi;
    }

    public virtual DungeonLevel Generate(int levelIndex, LayouterOptions opt = null)
    {
      var mazeNodes = CreateDungeonNodes();
      var layouter = new DefaultNodeLayouter(container);
      var level = layouter.DoLayout(mazeNodes, opt);

      return level;
    }
  }
}
