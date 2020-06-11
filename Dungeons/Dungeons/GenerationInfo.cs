using System;
using System.Drawing;

namespace Dungeons
{
  /// <summary>
  /// Info describing creation of the dungeon node. Members public to enhance speed.
  /// </summary>
  public class GenerationInfo : ICloneable
  {
    //Number of rooms inside a level, not counting ChildIslands (smallers rooms inside a room)
    public int NumberOfRooms = 1;

    /// <summary>
    /// Normally true, can be set to false for issue testing purposes
    /// </summary>
    public bool CreateDoors = true;
    public EntranceSide? ForcedNextRoomSide;

    public int EntrancesCount = 0;
    public bool ChildIsland;

    //global switch
    public const bool ForceEmpty = false;

    public int ForcedNumberOfEnemiesInRoom { get; set; } = 0;//-1 means field is not used
    public bool GenerateOuterWalls = true && !ForceEmpty;
    public bool GenerateRandomInterior = true && !ForceEmpty;
    public bool GenerateRandomStonesBlocks = true && !ForceEmpty;
    public bool GenerateDoors = true && !ForceEmpty;
    internal bool GenerateEmptyTiles = true;

    public bool FirstNodeSmaller = false;
    const int minSize = 10;
    public Size MinNodeSize = new Size(minSize, minSize);
    public Size MaxNodeSize = new Size(12, 14);
    public Size ForcedChilldIslandSize = new Size(0, 0);

    public readonly int MinSubMazeNodeSize = 6;
    public readonly int MinSimpleInteriorSize = 3;
    public int MinRoomLeft = 6;

    public bool ChildIslandAllowed = true;
    public int MaxNumberOfChildIslands = 1;
    public bool ForceChildIslandInterior = false;

    public bool RevealTiles { get; set; } = false;
    public bool RevealAllNodes { get; set; } = false;

    public GenerationInfo()
    {
    }

    public virtual void MakeEmpty()
    {
      GenerateRandomInterior = false;
      GenerateRandomStonesBlocks = false;
      //GenerateDoors = false;
    }

    public virtual object Clone()
    {
      return this.MemberwiseClone() as GenerationInfo;
    }
  }
}
