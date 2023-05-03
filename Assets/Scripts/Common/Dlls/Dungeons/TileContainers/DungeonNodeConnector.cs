using Dungeons.Core;
using Dungeons.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons.TileContainers
{
  public struct RoomPlacementInfo
  {
    public Size Size { get; set; }
    public Point Position;
    public RoomPlacement Placement;
  }

  public class DungeonNodeConnector
  {
    public Point Node1Position;
    public Point Node2Position;
    Dictionary<RoomPlacement, RoomPlacementInfo> roomPlacementInfos = new Dictionary<RoomPlacement, RoomPlacementInfo>();

    int[] wallLength = { 0, 1, 2, 3 };
    const int size = GenerationInfo.MaxRoomSideSize * 2 + 4;
    public int upperLength = RandHelper.Random.Next(size, size);
    public int bottomLength = RandHelper.Random.Next(size, size);
    public int rightLength = RandHelper.Random.Next(size, size);
    public int leftLength = RandHelper.Random.Next(size, size);
    public readonly int centralRoomPosition = (int)(GenerationInfo.MaxRoomSideSize + 2);

    Dungeons.IDungeonGenerator dungeonGenerator;
    GenerationInfo infoCorridor;
    public const int CorridorNodeIndexStart = 6;
    int corridorNodeNextIndexStart = CorridorNodeIndexStart;

    public DungeonNodeConnector(Container cont, Dungeons.IDungeonGenerator dg, GenerationInfo info = null)
    {
      dungeonGenerator = dg;
      this.infoCorridor = info;
      if (this.infoCorridor == null)
        this.infoCorridor = cont.GetInstance<GenerationInfo>(); 
    }

    public DungeonNode ConnectNodes(List<DungeonNode> nodes, DungeonNode node1, DungeonNode node2)
    {
      Node1Position = GetNodePosition(node1);
      Node2Position = GetNodePosition(node2);

      infoCorridor.NumberOfRooms = 1;
      var roomPlacementInfo = CalculateCorridorPosition(nodes, node1, node2);
      
      var corridor = dungeonGenerator.CreateDungeonNodeInstance();
      corridor.Corridor = true;
      corridor.Create(roomPlacementInfo.Size.Width, roomPlacementInfo.Size.Height, infoCorridor, corridorNodeNextIndexStart++);
      corridor.Placement = roomPlacementInfo.Placement;
      var doors = corridor.GenerateDoors(roomPlacementInfo.Placement);
      
      dungeonGenerator.GenerateRoomContent(corridor.NodeIndex, infoCorridor, corridor);

      return corridor;
    }

    public RoomPlacementInfo CalculateCorridorPosition(List<DungeonNode> nodes, DungeonNode node1, DungeonNode node2)
    {
      var roomPlacementInfo = new RoomPlacementInfo();
      RoomPlacement roomPlacement = RoomPlacement.Unset;
      if (node1.Placement == RoomPlacement.LeftUpper && node2.Placement == RoomPlacement.RightUpper) //LeftUpper->RightUpper
      {
        roomPlacementInfo.Size = new Size(Node2Position.X - node1.Width + wallLength[3] - 1, 4);
        roomPlacementInfo.Position = new Point(node1.Width - wallLength[1], Node1Position.Y + (node1.Height / 2) - wallLength[1]);
        roomPlacement = RoomPlacement.CorrindorLeftUpperToRightUpper;

      }
      else if (node1.Placement == RoomPlacement.RightUpper && node2.Placement == RoomPlacement.RightLower)
      {
        var he = Node2Position.Y - node1.Height + wallLength[3] - 1;
        roomPlacementInfo.Size = new Size(4, he);
        roomPlacementInfo.Position = new Point(Node1Position.X + (node1.Width / 2) - wallLength[1], node1.Height - wallLength[1]);
        roomPlacement = RoomPlacement.CorrindorRightUpperToRightLower;
      }
      else if (node1.Placement == RoomPlacement.RightLower && node2.Placement == RoomPlacement.LeftLower)
      {
        roomPlacementInfo.Size = new Size(bottomLength - node2.Width + wallLength[3] - 1, 4);
        roomPlacementInfo.Position = new Point(node2.Width - wallLength[1], Node2Position.Y + (node2.Height / 2) - wallLength[1]);
        roomPlacement = RoomPlacement.CorrindorRightLowerToLeftLower;
      }
      else if (node1.Placement == RoomPlacement.LeftLower && node2.Placement == RoomPlacement.LeftUpper)
      {
        var he = Node1Position.Y - node2.Height + wallLength[3] - 1;
        roomPlacementInfo.Size = new Size(4, he);
        roomPlacementInfo.Position = new Point((node2.Width / 2) - wallLength[1], node2.Height - wallLength[1]);
        roomPlacement = RoomPlacement.CorrindorLeftLowerToLeftUpper;
      }
      else
      {
        DebugHelper.Assert(node2.Placement == RoomPlacement.Center);
        if (node1.Placement == RoomPlacement.CorrindorLeftLowerToLeftUpper)
        {
          var centerNode = nodes[(int)RoomPlacement.Center];
          var leftUpperToLower = roomPlacementInfos[node1.Placement];
          var wi = GetNodePosition(centerNode).X - (leftUpperToLower.Position.X + leftUpperToLower.Size.Width - 2);

          roomPlacementInfo.Size = new Size(wi, 4);
          roomPlacementInfo.Position = new Point((nodes[(int)RoomPlacement.LeftUpper].Width / 2) + wallLength[1] + 1, centralRoomPosition + (node2.Height / 2) - wallLength[1]);
          roomPlacement = RoomPlacement.CorrindorCenterToLeft;
        }
        else if (node1.Placement == RoomPlacement.CorrindorLeftUpperToRightUpper)
        {
          roomPlacementInfo.Size = new Size(4, centralRoomPosition - (nodes[(int)RoomPlacement.LeftUpper].Height / 2) + wallLength[1] - 2);
          roomPlacementInfo.Position = new Point(centralRoomPosition + (node2.Width / 2) - wallLength[1], (nodes[(int)RoomPlacement.LeftUpper].Height / 2) + wallLength[1] + 1);
          roomPlacement = RoomPlacement.CorrindorCenterToUpper;
        }
        else if (node1.Placement == RoomPlacement.CorrindorRightUpperToRightLower)
        {
          var width = upperLength - centralRoomPosition - node2.Width + (nodes[(int)RoomPlacement.RightUpper].Width / 2) + wallLength[2];
          roomPlacementInfo.Size = new Size(width - 1, 4);
          roomPlacementInfo.Position = new Point(centralRoomPosition + node2.Width - wallLength[1], centralRoomPosition + (node2.Height / 2) - wallLength[1]);
          roomPlacement = RoomPlacement.CorrindorCenterToRight;
        }
        else if (node1.Placement == RoomPlacement.CorrindorRightLowerToLeftLower)
        {
          roomPlacementInfo.Size = new Size(4, leftLength - centralRoomPosition + (nodes[(int)RoomPlacement.LeftLower].Height / 2) - node2.Height + wallLength[2] - 1);
          roomPlacementInfo.Position = new Point(centralRoomPosition + (node2.Width / 2) - wallLength[1], centralRoomPosition + node2.Height - wallLength[1]);
          roomPlacement = RoomPlacement.CorrindorCenterToLower;
        }
      }
      roomPlacementInfo.Placement = roomPlacement;
      roomPlacementInfos[roomPlacement] = roomPlacementInfo;
      return roomPlacementInfo;
    }

    public RoomPlacementInfo GetRoomPlacementInfo(RoomPlacement roomPlacement)
    {
      return roomPlacementInfos[roomPlacement];
    }
        
    public Point GetNodePosition(DungeonNode node)
    {
      var position = new Point();
      if (node.Placement == RoomPlacement.LeftUpper)
        position = new Point(0, 0);

      if (node.Placement == RoomPlacement.RightUpper)
        position = new Point(upperLength, 0);

      if (node.Placement == RoomPlacement.LeftLower)
        position = new Point(0, leftLength);

      if (node.Placement == RoomPlacement.RightLower)
        position = new Point(bottomLength, rightLength);

      if (node.Placement == RoomPlacement.Center)
        position = new Point(centralRoomPosition, centralRoomPosition);
      return position;
    }
  }
}

