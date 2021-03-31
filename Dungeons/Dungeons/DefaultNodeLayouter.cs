using Dungeons.Core;
using Dungeons.TileContainers;
using SimpleInjector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Dungeons
{
  interface INodeLayouter
  {
    DungeonLevel DoLayout(List<DungeonNode> nodes, LayouterOptions opt = null);
  }

  struct AppendNodeInfo 
  {
    public Point Position;
    public EntranceSide side;
    public DungeonNode DungeonNode;

    public AppendNodeInfo(DungeonNode node)
    {
      Position = new Point();
      side = EntranceSide.Left;
      DungeonNode = node;
    }

    public override string ToString()
    {
      return Position.X + ", " + Position.Y + " " + side + " " + DungeonNode;
    }
  }

  public class LayouterOptions
  {
    public bool RevealAllNodes { get; set; } = true;
  }
  
  //Takes list of nodes and arranges them into a dungeon. Nodes are aligning one to another no special corridors.
  class DefaultNodeLayouter : INodeLayouter
  {
    int nodesPadding = 0;
    bool generateLayoutDoors = true;
    LayouterOptions options;
    Container container;

    public DefaultNodeLayouter(Container container, GenerationInfo info = null)
    {
      this.container = container;
    }

    public DungeonLevel DoLayout(List<DungeonNode> nodes, LayouterOptions opt = null) 
    {
      if(nodes.Any(i=> !i.Created))
        return container.GetInstance<DungeonLevel>();
      options = opt;
      if (options == null)
        options = new LayouterOptions();
      //totals sizes
      var tw = nodes.Sum(i => i.Width);
      var th = nodes.Sum(i => i.Height);

      var gi = new GenerationInfo();
      gi.GenerateOuterWalls = false;
      gi.GenerateRandomInterior = false;
      gi.GenerateEmptyTiles = false;
      var localLevel = container.GetInstance<DungeonNode>();
      localLevel.Container = this.container;
      //Activator.CreateInstance(typeof(T), new object[] { container }) as T;
      localLevel.Create(tw, th /*+ nodesPadding * nodes.Count*/, gi, -1, null, false);
      
      var maxLoc = localLevel.GetMaxXY();
      if (nodes.Count > 1)
      {
        if(nodes[1].GetTiles().First().DungeonNodeIndex != nodes[1].NodeIndex)
          container.GetInstance<ILogger>().LogError("nodes[1].GetTiles().First().DungeonNodeIndex != nodes[1].NodeIndex");
      }

      LayoutNodes(localLevel, nodes);

      if (localLevel.Tiles[0, 0].DungeonNodeIndex == DungeonNode.DefaultNodeIndex)
      {
        container.GetInstance<ILogger>().LogError("localLevel.Tiles[0, 0].DungeonNodeIndex == "+ DungeonNode.DefaultNodeIndex);
      }

      var max = localLevel.GetMaxXY();

      var level = container.GetInstance<DungeonLevel>();
      var width = max.Item1 + 1;
      var height = max.Item2 + 1;
      level.Create(width, height);
      level.AppendMaze(localLevel, new Point(0, 0), new Point(width, height));
      level.DeleteWrongDoors();

      level.SecretRoomIndex = -1;
      var sn = nodes.Where(i => i.Secret).FirstOrDefault();
      if(sn!=null)
        level.SecretRoomIndex = sn.NodeIndex;
      return level;
    }

    DungeonNode lastNonSecretNode;
    List<DungeonNode> mazeNodes;
    Dictionary<DungeonNode, AppendNodeInfo> appendNodeInfos = new Dictionary<DungeonNode, AppendNodeInfo>();

    protected virtual void LayoutNodes(DungeonNode level, List<DungeonNode> mazeNodes)
    {
      this.mazeNodes = mazeNodes;
      var info = new AppendNodeInfo(mazeNodes[0]);
      info.side = EntranceSide.Right;
      
      float chanceForLevelTurn = 0.5f;
      EntranceSide? prevEntranceSide = null;
      var secretRoomIndex = mazeNodes.FindIndex(i => i.Secret);

      for (int currentNodeIndex = 0; currentNodeIndex < mazeNodes.Count; currentNodeIndex++)
      {
        appendNodeInfos.Add(mazeNodes[currentNodeIndex], info);

        var currentNode = mazeNodes[currentNodeIndex];
        currentNode.Reveal(options.RevealAllNodes, true);
        if (!currentNode.Secret)
          lastNonSecretNode = currentNode;


        bool shallBreak = currentNodeIndex == mazeNodes.Count - 1;
        AppendNodeInfo infoNext = new AppendNodeInfo();
        
        if (!shallBreak)
        {
          infoNext = CalcNextValues(info, chanceForLevelTurn, currentNodeIndex);

          if (currentNodeIndex < mazeNodes.Count - 1 && generateLayoutDoors)
          {
            var nextMaze = mazeNodes[currentNodeIndex + 1];
            var secretRoom = currentNodeIndex == 0 ? currentNode.Secret : nextMaze.Secret;
            if (!secretRoom)
              secretRoom = nextMaze.Secret;

            //this call must be done before AppendMaze because AppendMaze changes tiles x,y
            List<Tiles.IDoor> doors = null;
            if(!currentNode.Secret || currentNode.NodeIndex == 0)
              doors = currentNode.GenerateLayoutDoors(infoNext.side, nextMaze.NodeIndex, secretRoom);
            if (currentNode.Secret)
            {
              if (currentNode.NodeIndex == 0)
                (doors[0]).DungeonNodeIndex = 1;
            }

            if (nextMaze.Secret && mazeNodes.Count > currentNodeIndex+2)
            {
              doors = currentNode.GenerateLayoutDoors(infoNext.side == EntranceSide.Bottom ? EntranceSide.Right : EntranceSide.Bottom , nextMaze.NodeIndex, false, true);
            }
          }
        }

        EntranceSide? entranceSideToSkip = null;
        if (secretRoomIndex == 0 && currentNodeIndex == 1)
          entranceSideToSkip = null;
        else
        {
          entranceSideToSkip = null;
          if (currentNodeIndex > 0)
            entranceSideToSkip = info.side == EntranceSide.Bottom ? EntranceSide.Top : EntranceSide.Left;
        }
        level.AppendMaze
        (
          currentNode,
          info.Position,
          null,
          false,
          entranceSideToSkip,
          currentNodeIndex > 0 ? mazeNodes[currentNodeIndex - 1] : null
        );

        if (shallBreak)
          break;
        entranceSideToSkip = null;
        prevEntranceSide = infoNext.side;
        info = infoNext;
      }
    }
        
    private AppendNodeInfo CalcNextValues(AppendNodeInfo currentAppendInfo, float chanceForLevelTurn, int currentNodeIndex)
    {
      var currentNode = mazeNodes[currentNodeIndex];
      //copy current append info (struct)
      AppendNodeInfo nextAppendInfo = currentAppendInfo;
      
      nextAppendInfo.DungeonNode = mazeNodes[currentNodeIndex+1];
      DungeonNode sizeProvider  = currentAppendInfo.DungeonNode;

      
      if (currentNode.Secret)
      {
        if (currentNode.NodeIndex == 0)
          nextAppendInfo.side = GetRandSide();
        else
        { 
          if (currentAppendInfo.side == EntranceSide.Bottom)
              nextAppendInfo.side = EntranceSide.Right;
          else if (currentAppendInfo.side == EntranceSide.Right)
            nextAppendInfo.side = EntranceSide.Bottom;
        }
        if (lastNonSecretNode != null)
        {
          sizeProvider = lastNonSecretNode;
          nextAppendInfo.Position = appendNodeInfos[lastNonSecretNode].Position;
        }
      }
      else
      {
        //if (currentNodeIndex == 0)
        //  nextAppendInfo.side = EntranceSide.Bottom;
        //else
          nextAppendInfo.side = CalcSide(currentAppendInfo.side, nextAppendInfo.side, chanceForLevelTurn);
      }

      var pt = nextAppendInfo.Position;
      if (nextAppendInfo.side == EntranceSide.Bottom)
      {
        pt.Y += sizeProvider.Height -1 + nodesPadding;
      }
      else if (nextAppendInfo.side == EntranceSide.Right)
      {
        pt.X += sizeProvider.Width - 1;
      }
      nextAppendInfo.Position = pt;

      return nextAppendInfo;
    }

    private static EntranceSide CalcSide(EntranceSide current, EntranceSide next, float chanceForLevelTurn)//, int currentNodeIndex)
    {
      EntranceSide side = GetRandSide();
      if (current == next)
      {
        if (RandHelper.GetRandomDouble() >= chanceForLevelTurn)
          side = side == EntranceSide.Bottom ? EntranceSide.Right : EntranceSide.Bottom;
      }

      return side;
    }

    private static EntranceSide GetRandSide()
    {
      return RandHelper.GetRandomDouble() >= .5f ? EntranceSide.Bottom : EntranceSide.Right;
    }
  }
}
