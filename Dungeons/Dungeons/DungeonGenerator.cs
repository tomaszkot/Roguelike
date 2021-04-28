using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons
{
  public interface IDungeonGenerator
  {
    DungeonLevel Generate(int levelIndex, Dungeons.GenerationInfo info = null, LayouterOptions opt = null);
  }

  public class DungeonGenerator : IDungeonGenerator
  {
    static protected Random random;
    protected List<DungeonNode> nodes;
    private Container container;
    public Func<int, GenerationInfo, DungeonNode> CustomNodeCreator;

    public Container Container { get => container; }

    public event EventHandler<DungeonNode> NodeCreated;

    static DungeonGenerator()
    {
      random = new Random();
    }

    public DungeonGenerator(Container container)
    {
      this.container = container;
      container.GetInstance<ILogger>().LogInfo("DungeonGenerator ctor [container]: " + Container.GetHashCode());
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
      DungeonNode node;
      if (CustomNodeCreator != null)
      {
        node = CustomNodeCreator(nodeIndex, gi);
      }
      else
      {
        var minNodeSize = gi.MinNodeSize;
        var maxNodeSize = gi.MaxNodeSize;
        if (secretRoomIndex == nodeIndex
          )
        {
          minNodeSize.Width -= 1;
          minNodeSize.Height -= 1;

          maxNodeSize = minNodeSize;
        }
        var width = random.Next(minNodeSize.Width, maxNodeSize.Width);
        var height = random.Next(minNodeSize.Height, maxNodeSize.Height);

        node = CreateNode(width, height, gi, nodeIndex);
        if (secretRoomIndex == nodeIndex)
          node.Secret = true;
      }
      if (NodeCreated != null)
        NodeCreated(this, node);
      return node;
    }

    protected DungeonNode CreateNode(int w, int h, GenerationInfo gi, int nodeIndex)
    {
      DungeonNode dungeon = CreateDungeonNodeInstance();
      dungeon.Secret = this.secretRoomIndex == nodeIndex;
      dungeon.ChildIslandCreated += Dungeon_ChildIslandCreated;

      OnCreate(dungeon, w, h, gi, nodeIndex);
      return dungeon;
    }

    protected virtual void OnCreate(DungeonNode dungeon, int w, int h, GenerationInfo gi, int nodeIndex)
    {
      dungeon.Create(w, h, gi, nodeIndex);
    }

    public virtual DungeonNode CreateDungeonNodeInstance()
    {
      var node = container.GetInstance<DungeonNode>();
      node.Container = container;
      return node;
    }

    private void Dungeon_ChildIslandCreated(object sender, ChildIslandCreationInfo e)
    {
      OnChildIslandCreated(e);
    }

    protected virtual void OnChildIslandCreated(ChildIslandCreationInfo e)
    {
    }

    protected virtual DungeonNode CreateLevel(int levelIndex, int w, int h, GenerationInfo gi)
    {
      var dungeon = container.GetInstance<DungeonNode>();
      dungeon.Create(w, h, gi);
      return dungeon;
    }

    protected int secretRoomIndex = -1;
    //TODO public
    public virtual List<DungeonNode> CreateDungeonNodes(GenerationInfo info = null)
    {
      nodes = new List<DungeonNode>();
      var gi = info ?? this.CreateLevelGenerationInfo();

      if (!gi.PreventSecretRoomGeneration)
      {
        if (gi.SecretRoomIndex >= 0)
          secretRoomIndex = gi.SecretRoomIndex;
        else
          secretRoomIndex = RandHelper.GetRandomInt(gi.NumberOfRooms);
      }
      for (int i = 0; i < gi.NumberOfRooms; i++)
      {
        if (i > 0)
          gi.RevealTiles = false;
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

    public virtual DungeonLevel Generate(int levelIndex, GenerationInfo info = null, LayouterOptions opt = null)
    {
      var mazeNodes = CreateDungeonNodes(info);
      var diffIndexes = mazeNodes.GroupBy(i => i.NodeIndex).Count();
      if (diffIndexes != mazeNodes.Count)
      {
        container.GetInstance<Logger>().LogError("diffIndexes != mazeNodes.Count", false);
      }
      //var layouter = new CorridorNodeLayouter(container);
      var layouter = new DefaultNodeLayouter(container, info);
      var level = layouter.DoLayout(mazeNodes, opt);

      return level;
    }
  }
}
