using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Dungeons
{
  public interface IDungeonGenerator
  {
    DungeonLevel Generate(int levelIndex, Dungeons.GenerationInfo info = null, LayouterOptions opt = null);
    void GenerateRoomContent(int nodeIndex, Dungeons.GenerationInfo gi, DungeonNode node);

    //List<DungeonNode> CreateDungeonNodes(Dungeons.GenerationInfo info = null);
    DungeonNode CreateDungeonNodeInstance();
  }

  public class DungeonGenerator : IDungeonGenerator
  {
    static protected Random random;
    protected List<DungeonNode> nodes;
    private Container container;
    public Func<int, GenerationInfo, DungeonNode> CustomNodeCreator;
    public Container Container { get => container; }
    public GenerationInfo Info 
    { 
      get => info; 
      private set => info = value; 
    }

    public event EventHandler<DungeonNode> NodeCreated;
    GenerationInfo info;

    static DungeonGenerator()
    {
      random = new Random();
    }

    public DungeonGenerator(Container container)
    {
      this.container = container;
      //container.GetInstance<ILogger>().LogInfo("DungeonGenerator ctor [container]: " + Container.GetHashCode());
    }

    public virtual void GenerateRoomContent(int nodeIndex, Dungeons.GenerationInfo gi, DungeonNode node)
    {

    }

    //Tile GetPossibleDoorTile(List<Tile> listOne, List<Tile> listTwo)
    //{
    //  var common = listOne.SelectMany(x => listTwo.Where(y => y.IsAtSamePosition(x))).ToList();
    //  int doorIndex = random.Next(common.Count);
    //  if (doorIndex == 0)
    //    doorIndex++;
    //  if (doorIndex == common.Count - 1)
    //    doorIndex--;
    //  return common[doorIndex];
    //}

    //DungeonNode CreateNode(int nodeIndex)
    //{
    //  GenerationInfo gi = container.GetInstance<GenerationInfo>(); 
    //  return CreateNode(nodeIndex, gi);
    //}

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

    protected virtual DungeonNode CreateNode(int w, int h, GenerationInfo gi, int nodeIndex)
    {
      DungeonNode dungeon = CreateDungeonNodeInstance();

      dungeon.ChildIslandCreated += Dungeon_ChildIslandCreated;
      if (dungeonLayouterKind == DungeonLayouterKind.Corridor && nodeIndex == (int)RoomPlacement.Center)
      {
        w = GenerationInfo.MaxRoomSideSize;
        h = GenerationInfo.MaxRoomSideSize;
      }
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
    protected virtual List<DungeonNode> CreateDungeonNodes(GenerationInfo info = null)
    {
      nodes = new List<DungeonNode>();
      this.Info = info ?? container.GetInstance<GenerationInfo>();

      if (!this.info.PreventSecretRoomGeneration)
      {
        if (this.dungeonLayouterKind == DungeonLayouterKind.Default)
        {
          if (this.info.SecretRoomIndex >= 0)
            secretRoomIndex = this.info.SecretRoomIndex;
          else
          {
            secretRoomIndex = RandHelper.GetRandomInt(this.info.NumberOfRooms);
            if (secretRoomIndex == 0)
              secretRoomIndex = 1;//TODO
          }
        }
      }
      container.GetInstance<ILogger>().LogInfo("CreateDungeonNodes secretRoomIndex: " + secretRoomIndex);

      for (int i = 0; i < this.info.NumberOfRooms; i++)
      {
        if (i > 0)
          this.info.RevealTiles = false;
        var node = CreateNode(i, this.info);
        nodes.Add(node);

      }
      return nodes;
    }

    DungeonLayouterKind dungeonLayouterKind;
    public virtual DungeonLevel Generate(int levelIndex, GenerationInfo info = null, LayouterOptions opt = null)
    {
      if (info == null)
      {
        info = Container.GetInstance<GenerationInfo>();
      }
      InitGenerationInfo(info);
      INodeLayouter layouter = null;
      dungeonLayouterKind = info.ForcedDungeonLayouterKind;
      if (dungeonLayouterKind == DungeonLayouterKind.Unset)
      {
        dungeonLayouterKind = CalcLayouterKind();
      }

      if (dungeonLayouterKind == DungeonLayouterKind.Corridor)
      {
        info.NumberOfRooms = 5;
        layouter = new CorridorNodeLayouter(container, this, info);
      }
      else
        layouter = new DefaultNodeLayouter(container, info);

      Debug.WriteLine("dungeonLayouterKind: "+ dungeonLayouterKind);

      var mazeNodes = CreateDungeonNodes(info); 
      var diffIndexes = mazeNodes.GroupBy(i => i.NodeIndex).Count();
      if (diffIndexes != mazeNodes.Count)
      {
        container.GetInstance<ILogger>().LogError("diffIndexes != mazeNodes.Count", false);
      }

      var localLevel = layouter.DoLayout(mazeNodes, opt);
      

      var max = localLevel.GetMaxXY();

      var level = container.GetInstance<DungeonLevel>();
      var width = max.Item1 + 1;
      var height = max.Item2 + 1;
      level.Create(width, height);

      level.AppendMaze(localLevel, new Point(0, 0), new Point(width, height));
      level.DeleteWrongDoors();

      
      
      var sn = nodes.Where(i => i.Secret).FirstOrDefault();
      if (sn != null)
        level.SecretRoomIndex = sn.NodeIndex;


      DoPostGenerationJobs(level);

      return level;
    }

    protected virtual DungeonLayouterKind CalcLayouterKind()
    {
      if (RandHelper.GetRandomDouble() > 0.5f)
        return DungeonLayouterKind.Corridor;
      else
        return DungeonLayouterKind.Default;
    }

    protected virtual void CreateDynamicTiles(List<Dungeons.TileContainers.DungeonNode> mazeNodes)
    { 
    }

    protected virtual void InitGenerationInfo(GenerationInfo info)
    {
      
    }

    protected virtual void DoPostGenerationJobs(DungeonLevel level)
    {
      SetShadowedFlag(level);
    }

    protected void SetShadowedFlag(DungeonLevel level)
    {
      var walls = level.GetTiles<Wall>();
      foreach (var wall in walls)
      {
        var neibWalls = level.GetNeighborTiles(wall).Where(i => i is Wall || i is IDoor).ToList();
        if (neibWalls.Count == 3)
        {
          if (wall.Shadowed && wall.EntranceSide != EntranceSide.Unset
            )
          {
            var secret = level.GetTiles<IDoor>().Where(i => i.Secret).FirstOrDefault();
            if (secret != null)
            {
              var secretTile = secret as Tile;
              if (secretTile.point.Y == wall.point.Y)
                continue;
            }
            wall.Shadowed = false;
          }
        }
      }
    }
  }
}
