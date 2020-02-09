using Dungeons.Core;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons
{
  class CorridorNodeLayouter : INodeLayouter
  {
    int nodesPadding = 0;
    bool generateLayoutDoors = true;
    EntranceSide? forcedNextSide = null;//EntranceSide.Bottom;
    EntranceSide? forcedEntranceSideToSkip = null;// EntranceSide.Right;
    LayouterOptions options;
    Container container;

    public CorridorNodeLayouter(Container container)
    {
      this.container = container;
    }

    public DungeonLevel DoLayout(List<DungeonNode> nodes, LayouterOptions opt = null)
    {
      List<DungeonNode> finalNodes = nodes;

      options = opt;
      if (options == null)
        options = new LayouterOptions();
      //totals sizes
      var tw = finalNodes.Sum(i => i.Width);
      var th = finalNodes.Sum(i => i.Height);

      var localLevel = container.GetInstance<DungeonLevel>();
      localLevel.Create(tw , th );
      int nextX = 0;
      int nextY = 0;
      int nodeIndex = 0;
      foreach (var node in finalNodes)
      {
        localLevel.AppendMaze
        (
          node,
          new Point(nextX, nextY),
          null,
          false,
          EntranceSide.Left,
          nodeIndex > 0 ? finalNodes[nodeIndex - 1] : null
        );
        nextX += node.Width;
        nodeIndex++;
        //nextY += node.Height;
      }
      //
     // var maxLoc = localLevel.GetMaxXY();
      var max = localLevel.GetMaxXY();

      var level = container.GetInstance<DungeonLevel>();
      level.Create(max.Item1 + 1, max.Item2 + 1);
      level.AppendMaze(localLevel, new Point(0, 0), new Point(max.Item1 + 1, max.Item2 + 1));
      level.DeleteWrongDoors();

      return level;
    }

    protected virtual void LayoutNodes(DungeonNode localLevel, List<DungeonNode> mazeNodes)
    {
      //AppendNodeInfo info = new AppendNodeInfo();
      //info.side = EntranceSide.Right;
      //float chanceForLevelTurn = 0.5f;
      //EntranceSide? prevEntranceSide = null;

      //for (int nodeIndex = 0; nodeIndex < mazeNodes.Count; nodeIndex++)
      //{
      //  var infoNext = CalcNextValues(mazeNodes, info, chanceForLevelTurn, nodeIndex);
      //  if (nodeIndex < mazeNodes.Count - 1 && generateLayoutDoors)
      //    mazeNodes[nodeIndex].GenerateLayoutDoors(infoNext.side, mazeNodes[nodeIndex + 1]);

      //  EntranceSide? entranceSideToSkip = null;
      //  if (nodeIndex > 0)
      //  {
      //    if (prevEntranceSide == EntranceSide.Right)
      //      entranceSideToSkip = EntranceSide.Left;
      //    else if (prevEntranceSide == EntranceSide.Bottom)
      //      entranceSideToSkip = EntranceSide.Top;
      //    else
      //      Debug.Assert(false);
      //  }
      //  if (forcedEntranceSideToSkip != null)
      //    entranceSideToSkip = forcedEntranceSideToSkip.Value;

      //  mazeNodes[nodeIndex].Reveal(options.RevealAllNodes, true);

      //  localLevel.AppendMaze
      //  (
      //    mazeNodes[nodeIndex],
      //    new Point(info.nextX, info.nextY),
      //    null,
      //    false,
      //    entranceSideToSkip,
      //    nodeIndex > 0 ? mazeNodes[nodeIndex - 1] : null
      //  );

      //  prevEntranceSide = infoNext.side;
      //  info = infoNext;
      //}
    }
    
    //private AppendNodeInfo CalcNextValues(List<DungeonNode> mazeNodes, AppendNodeInfo prevInfo, float chanceForLevelTurn, int nodeIndex)
    //{
    //  AppendNodeInfo infoNext = prevInfo;

    //  if (prevInfo.nextForcedSide != null)
    //  {
    //    infoNext.side = prevInfo.nextForcedSide.Value;
    //    //nextForcedSide = null;
    //  }
    //  else
    //  {
    //    if (forcedNextSide != null)
    //      infoNext.side = forcedNextSide.Value;
    //    else
    //    {
    //      infoNext.side = RandHelper.GetRandomDouble() >= .5f ? EntranceSide.Bottom : EntranceSide.Right;
    //      if (nodeIndex > 0 && prevInfo.side == infoNext.side)
    //      {
    //        if (RandHelper.GetRandomDouble() >= chanceForLevelTurn)
    //          infoNext.side = prevInfo.side == EntranceSide.Bottom ? EntranceSide.Right : EntranceSide.Bottom;
    //      }
    //      chanceForLevelTurn -= 0.15f;
    //    }
    //  }
    //  if (infoNext.side == EntranceSide.Bottom)
    //  {
    //    infoNext.nextY += mazeNodes[nodeIndex].Height - 1 + nodesPadding;

    //  }
    //  else if (infoNext.side == EntranceSide.Right)
    //  {
    //    infoNext.nextX += mazeNodes[nodeIndex].Width - 1;
    //  }

    //  return infoNext;
    //}
  }
}

