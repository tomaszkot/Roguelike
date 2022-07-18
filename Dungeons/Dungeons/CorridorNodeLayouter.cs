using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Dungeons
{
  class CorridorNodeLayouter : INodeLayouter
  {
    //int nodesPadding = 0;
    //bool generateLayoutDoors = true;
    //EntranceSide? forcedNextSide = null;//EntranceSide.Bottom;
    //EntranceSide? forcedEntranceSideToSkip = null;// EntranceSide.Right;
    LayouterOptions options;
    Container container;
    GenerationInfo info;
    DungeonNode lastNonSecretNode;
    List<DungeonNode> mazeNodes;
    Dictionary<DungeonNode, AppendNodeInfo> appendNodeInfos = new Dictionary<DungeonNode, AppendNodeInfo>();
    int nodesPadding = 0;
    bool generateLayoutDoors = true;
    public enum RoomPlacement { LeftUpper = 0, RightUpper = 2, LeftLower = 1, RightLower = 3, Center = 4, CorrindorHorizontal = 5, CorrindorVertical = 6 }
    public CorridorNodeLayouter(Container container, GenerationInfo info = null)
    {
      this.container = container;
      this.info = info;
    }

    public DungeonLevel DoLayout(List<DungeonNode> nodes, LayouterOptions opt = null)
    {
      if (nodes.Any(i => !i.Created))
        return container.GetInstance<DungeonLevel>();
      options = opt;
      if (options == null)
        options = new LayouterOptions();
      //totals sizes
      var tw = nodes.Sum(i => i.Width);
      tw += 15; //SK
      var th = nodes.Sum(i => i.Height);
      th += 20;

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
        if (nodes[1].GetTiles().First().DungeonNodeIndex != nodes[1].NodeIndex)
          container.GetInstance<ILogger>().LogError("nodes[1].GetTiles().First().DungeonNodeIndex != nodes[1].NodeIndex");
      }

      LayoutNodes(localLevel, nodes);

      if (localLevel.Tiles[0, 0] != null && localLevel.Tiles[0, 0].DungeonNodeIndex == DungeonNode.DefaultNodeIndex)
      {
        container.GetInstance<ILogger>().LogError("localLevel.Tiles[0, 0].DungeonNodeIndex == " + DungeonNode.DefaultNodeIndex);
      }

      var max = localLevel.GetMaxXY();

      var level = container.GetInstance<DungeonLevel>();
      var width = max.Item1 + 1;
      var height = max.Item2 + 1;
      level.Create(width, height);

      //var doors = localLevel.GetTiles<IDoor>();
      //var secret = doors.Any(i => i.Secret);
      //if(!info.PreventSecretRoomGeneration)
      //  Debug.Assert(secret);
      level.AppendMaze(localLevel, new Point(0, 0), new Point(width, height));
      level.DeleteWrongDoors();
      //if (secret)
      //{
      //  var secretAppended = level.GetTiles<IDoor>().Any(i => i.Secret);
      //  Debug.Assert(secretAppended);
      //}

      level.SecretRoomIndex = -1;
      var sn = nodes.Where(i => i.Secret).FirstOrDefault();
      if (sn != null)
        level.SecretRoomIndex = sn.NodeIndex;
      return level;
    }

    protected virtual void LayoutNodes(DungeonNode level, List<DungeonNode> mazeNodes)
    {
      this.mazeNodes = mazeNodes;
      var random = RandHelper.Random;
      const int centralRoomPosition = 12;
      int[] wallLenght = { 0, 1, 2, 3 };
      List<int[]> positionsOfNodes = new List<int[]>();

      var currentWidth = 0;
      var currentHeight = 0;
      List<int> distances = new List<int>();
      mazeNodes[0].GenerateDoors(RoomPlacement.LeftUpper);
      mazeNodes[1].GenerateDoors(RoomPlacement.LeftLower);
      mazeNodes[2].GenerateDoors(RoomPlacement.RightUpper);
      mazeNodes[3].GenerateDoors(RoomPlacement.RightLower);
      mazeNodes[4].GenerateDoors(RoomPlacement.Center);
      for (int i = 0; i < mazeNodes.Count; i++) //creating rooms
      {
        if (i == (int)RoomPlacement.LeftUpper || i == (int)RoomPlacement.RightUpper)
        {
          positionsOfNodes.Add(new int[]{ currentWidth,currentHeight});
          currentHeight += random.Next(24, 28);
          distances.Add(currentHeight);
        }
        if (i == (int)RoomPlacement.LeftLower || i == (int)RoomPlacement.RightLower)
        {
          if (i == (int)RoomPlacement.RightLower)
            currentWidth = random.Next(24, 28);
          positionsOfNodes.Add(new int[] { currentWidth, currentHeight });
          currentHeight = 0;
          if (currentWidth == 0)
            currentWidth += random.Next(24, 28);
          distances.Add(currentWidth);
        }
      }
      positionsOfNodes.Add(new int[] { centralRoomPosition, centralRoomPosition });


      var d = new DungeonGenerator(container);
      var infoC = new GenerationInfo();
      infoC.NumberOfRooms = 1;
      currentWidth = 0;
      currentHeight = 0;
      var h = 0;
      var h1 = 0;
      for (int i = 0; i < 4; i++) //creating corrindors
      {
        if (i % 2 == 0)
        {
          infoC.MinNodeSize = new Size(distances[i+1] - mazeNodes[h].Width + wallLenght[3], 4);
          infoC.MaxNodeSize = new Size(distances[i+1] - mazeNodes[h].Width + wallLenght[3], 4);
          h += 1;
        }
        else {
          infoC.MinNodeSize = new Size(4, distances[h1] - mazeNodes[h1].Height + wallLenght[3]);
          infoC.MaxNodeSize = new Size(4, distances[h1] - mazeNodes[h1].Height + wallLenght[3]);
          h1 += 2;
        }
        var corrindorNodes = d.CreateDungeonNodes(infoC);
        mazeNodes.Add(corrindorNodes[0]);
      }


      infoC.MinNodeSize = new Size(4, centralRoomPosition - (mazeNodes[(int)RoomPlacement.LeftUpper].Height / 2) + wallLenght[1]); //creating corrindors to island
      infoC.MaxNodeSize = new Size(4, centralRoomPosition - (mazeNodes[(int)RoomPlacement.LeftUpper].Height / 2) + wallLenght[1]);
      var corrindorIslandNotes = d.CreateDungeonNodes(infoC);
      mazeNodes.Add(corrindorIslandNotes[0]);

      infoC.MinNodeSize = new Size(4, (mazeNodes[(int)RoomPlacement.LeftLower].Height / 2) + distances[0] - centralRoomPosition - mazeNodes[4].Height + wallLenght[2]);
      infoC.MaxNodeSize = new Size(4, (mazeNodes[(int)RoomPlacement.LeftLower].Height / 2) + distances[0] - centralRoomPosition - mazeNodes[4].Height + wallLenght[2]); ; ;
      corrindorIslandNotes = d.CreateDungeonNodes(infoC);
      mazeNodes.Add(corrindorIslandNotes[0]);

      infoC.MinNodeSize = new Size(centralRoomPosition - (mazeNodes[(int)RoomPlacement.LeftUpper].Width / 2) + wallLenght[2], 4);
      infoC.MaxNodeSize = new Size(centralRoomPosition - (mazeNodes[(int)RoomPlacement.LeftUpper].Width / 2) + wallLenght[2], 4);
      corrindorIslandNotes = d.CreateDungeonNodes(infoC);
      mazeNodes.Add(corrindorIslandNotes[0]);

      infoC.MinNodeSize = new Size(distances[1] - centralRoomPosition + wallLenght[1] - mazeNodes[4].Width + (mazeNodes[(int)RoomPlacement.RightUpper].Width / 2),4);
      infoC.MaxNodeSize = new Size(distances[1] - centralRoomPosition + wallLenght[1] - mazeNodes[4].Width + (mazeNodes[(int)RoomPlacement.RightUpper].Width / 2), 4);
      corrindorIslandNotes = d.CreateDungeonNodes(infoC);
      mazeNodes.Add(corrindorIslandNotes[0]);

      var heightOfPassage = 0;
      var widthOfPassage = 0;
      currentHeight = 0;
      currentWidth = 0;
      h = 0;
      h1 = 0;

      mazeNodes[5].GenerateDoors(RoomPlacement.CorrindorHorizontal);
      mazeNodes[6].GenerateDoors(RoomPlacement.CorrindorVertical);
      mazeNodes[7].GenerateDoors(RoomPlacement.CorrindorHorizontal);
      mazeNodes[8].GenerateDoors(RoomPlacement.CorrindorVertical);
      mazeNodes[9].GenerateDoors(RoomPlacement.CorrindorVertical);
      mazeNodes[10].GenerateDoors(RoomPlacement.CorrindorVertical);
      mazeNodes[11].GenerateDoors(RoomPlacement.CorrindorHorizontal);
      mazeNodes[12].GenerateDoors(RoomPlacement.CorrindorHorizontal);

      for (int i = 0; i<4;i++) //placing corrindors
      {
        if ((i % 2) == 0){
          heightOfPassage = ((mazeNodes[h].Height / 2) - wallLenght[1]) + currentHeight;
          widthOfPassage = mazeNodes[h].Width - wallLenght[1];
          h++;
          currentHeight = distances[0];
        }
        if ((i % 2) == 1){ //h
          heightOfPassage = mazeNodes[h1].Height - wallLenght[1];
          widthOfPassage = mazeNodes[h1].Width / 2 + currentWidth - wallLenght[2];
          currentWidth = distances[1];
          h1 += 2;
        }
          positionsOfNodes.Add(new int[] { widthOfPassage, heightOfPassage });
      }

      heightOfPassage = (mazeNodes[(int)RoomPlacement.LeftUpper].Height / 2) + wallLenght[1];
      widthOfPassage = centralRoomPosition + (mazeNodes[4].Width / 2) - wallLenght[1];
      positionsOfNodes.Add(new int[] { widthOfPassage, heightOfPassage });

      heightOfPassage = centralRoomPosition + mazeNodes[4].Height - wallLenght[1];
      positionsOfNodes.Add(new int[] { widthOfPassage, heightOfPassage });

      heightOfPassage = ((distances[0] - mazeNodes[(int)RoomPlacement.LeftUpper].Height)/2 + mazeNodes[(int)RoomPlacement.LeftUpper].Height) - wallLenght[1];
      widthOfPassage = (mazeNodes[(int)RoomPlacement.LeftUpper].Width / 2);
      positionsOfNodes.Add(new int[] { widthOfPassage, heightOfPassage });

      widthOfPassage = centralRoomPosition + mazeNodes[4].Width - wallLenght[1];
      positionsOfNodes.Add(new int[] { widthOfPassage, heightOfPassage });

      for(int i=0; i<mazeNodes.Count; i++) {
        level.AppendMaze(mazeNodes[i], new Point(positionsOfNodes[i][0],positionsOfNodes[i][1]));
      }
      mazeNodes.ForEach(p => p.Reveal(options.RevealAllNodes,true));
    }

    

    private EntranceSide CalcSide(EntranceSide current, EntranceSide next, float chanceForLevelTurn)//, int currentNodeIndex)
    {
      if (info != null && info.ForcedNextRoomSide != null)
        return info.ForcedNextRoomSide.Value;

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

