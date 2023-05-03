using System;
using System.Drawing;

namespace Dungeons
{
  /// <summary>
  /// Info describing creation of the dungeon node. Members public to enhance speed.
  /// </summary>
  public class GenerationInfo : ICloneable
  {
    public static bool DefaultRevealedValue = true;
    //Number of rooms inside a level, not counting ChildIslands (smallers rooms inside a room)
    public int NumberOfRooms = 4;
    public int SecretRoomIndex { get; set; } = -1;
    public bool PreventSecretRoomGeneration;

    /// <summary>
    /// Normally true, can be set to false for issue testing purposes
    /// </summary>
    public bool CreateDoors = true;
    public EntranceSide? ForcedNextRoomSide;

    public int EntrancesCount = 0;
    public bool ChildIsland;

    //global switch
    public const bool ForceEmpty = false;


    public bool GenerateOuterWalls = true && !ForceEmpty;
    private bool generateRandomInterior = true && !ForceEmpty;
    public bool GenerateRandomStonesBlocks = true && !ForceEmpty;
    public bool GenerateDoors = true && !ForceEmpty;
    internal bool GenerateEmptyTiles = true;

    public bool FirstNodeSmaller = false;
    public const int MinRoomSideSize = 16;
    public const int MaxRoomSideSize = 20;
    public Size MinNodeSize = new Size(MinRoomSideSize, MinRoomSideSize);
    public Size MaxNodeSize = new Size(MaxRoomSideSize, MaxRoomSideSize);
    public Size ForcedChilldIslandSize = new Size(0, 0);

    public readonly int MinSubMazeNodeSize = 7;
    public readonly int MinSimpleInteriorSize = 3;
    public int MinRoomLeft = 8;

    public bool ChildIslandAllowed = true;
    public int MaxNumberOfChildIslands = 1;
    public bool ForceChildIslandInterior = false;

    public bool RevealTiles { get; set; } = false;
    public bool RevealAllNodes { get; set; } = false;
    public bool GenerateRandomInterior 
    { 
      get => generateRandomInterior; 
      set => generateRandomInterior = value; 
    }

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
