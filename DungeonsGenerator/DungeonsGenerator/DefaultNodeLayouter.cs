﻿using Dungeons.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Dungeons
{
  struct AppendNodeInfo
  {
    public int nextX;
    public int nextY;
    public EntranceSide side;
    public EntranceSide? nextForcedSide;

    AppendNodeInfo(EntranceSide? nextForcedSide = null)
    {
      nextX = 0;
      nextY = 0;
      side = EntranceSide.Left;
      this.nextForcedSide = nextForcedSide;
    }
  }

  public class LayouterOptions
  {
    public bool RevealAllNodes { get; set; } = true;
  }
  
  //Takes list of nodes and arranges them into a dungeon. Nodes are aligning one to another no special corridors.
  class DefaultNodeLayouter
  {
    int nodesPadding = 0;
    bool generateLayoutDoors = true;
    EntranceSide? forcedNextSide = EntranceSide.Bottom;
    EntranceSide? forcedEntranceSideToSkip = null;// EntranceSide.Right;
    LayouterOptions options;

    public T DoLayout<T>(List<DungeonNode> nodes, LayouterOptions opt = null) where T : DungeonNode, new()
    {
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
      var localLevel = new DungeonNode(tw, th + nodesPadding * nodes.Count, gi, -1);
      
      var maxLoc = localLevel.GetMaxXY();

      LayoutNodes(localLevel, nodes);

      var max = localLevel.GetMaxXY();
      //generics sucks in C#
      var Level = System.Activator.CreateInstance(typeof(T), max.Item1 + 1, max.Item2 + 1) as T;
      Level.AppendMaze(localLevel, new Point(0, 0), new Point(max.Item1 + 1, max.Item2 + 1));
      Level.DeleteWrongDoors();

      return Level;
    }
    
    protected virtual void LayoutNodes(DungeonNode localLevel, List<DungeonNode> mazeNodes)
    {
      AppendNodeInfo info = new AppendNodeInfo();
      info.side = EntranceSide.Right;
      float chanceForLevelTurn = 0.5f;
      EntranceSide? prevEntranceSide = null;

      for (int nodeIndex = 0; nodeIndex < mazeNodes.Count; nodeIndex++)
      {
        var infoNext = CalcNextValues(mazeNodes, info, chanceForLevelTurn, nodeIndex);
        if(nodeIndex < mazeNodes.Count-1 && generateLayoutDoors)
          mazeNodes[nodeIndex].GenerateLayoutDoors(infoNext.side, mazeNodes[nodeIndex+1]);

        EntranceSide? entranceSideToSkip = null;
        if (nodeIndex > 0)
        {
          if (prevEntranceSide == EntranceSide.Right)
            entranceSideToSkip = EntranceSide.Left;
          else if (prevEntranceSide == EntranceSide.Bottom)
            entranceSideToSkip = EntranceSide.Top;
          else
            Debug.Assert(false);
        }
        if (forcedEntranceSideToSkip != null)
          entranceSideToSkip = forcedEntranceSideToSkip.Value;

        mazeNodes[nodeIndex].Reveal(options.RevealAllNodes, true);

        localLevel.AppendMaze
        (
          mazeNodes[nodeIndex],
          new Point(info.nextX, info.nextY),
          null,
          false,
          entranceSideToSkip,
          nodeIndex > 0 ? mazeNodes[nodeIndex - 1] : null
        );

        prevEntranceSide = infoNext.side;
        info = infoNext;
      }
    }

    

    private AppendNodeInfo CalcNextValues(List<DungeonNode> mazeNodes, AppendNodeInfo prevInfo, float chanceForLevelTurn, int nodeIndex)
    {
      AppendNodeInfo infoNext = prevInfo;

      if (prevInfo.nextForcedSide != null)
      {
        infoNext.side = prevInfo.nextForcedSide.Value;
        //nextForcedSide = null;
      }
      else
      {
        if (forcedNextSide != null)
          infoNext.side = forcedNextSide.Value;
        else
        {
          infoNext.side = RandHelper.GetRandomDouble() >= .5f ? EntranceSide.Bottom : EntranceSide.Right;
          if (nodeIndex > 0 && prevInfo.side == infoNext.side)
          {
            if (RandHelper.GetRandomDouble() >= chanceForLevelTurn)
              infoNext.side = prevInfo.side == EntranceSide.Bottom ? EntranceSide.Right : EntranceSide.Bottom;
          }
          chanceForLevelTurn -= 0.15f;
        }
      }
      if (infoNext.side == EntranceSide.Bottom)
      {
        infoNext.nextY += mazeNodes[nodeIndex].Height -1 + nodesPadding;

      }
      else if (infoNext.side == EntranceSide.Right)
      {
        infoNext.nextX += mazeNodes[nodeIndex].Width - 1;
      }

      return infoNext;
    }
  }
}
