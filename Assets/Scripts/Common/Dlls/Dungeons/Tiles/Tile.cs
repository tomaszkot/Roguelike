//#define DEBUG_PROPS
using Dungeons.Core;
using Dungeons.TileContainers;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Linq;


namespace Dungeons.Tiles
{
  public class Constants
  {
    public const int MinNormalNodeIndex = 0;

    public const char SymbolBackground = '.';
    public const char SymbolDoor = '+';
    public const char SymbolWall = '#';
  }

  //[XmlInclude(typeof(Wall))]
  [Serializable]
  public class Tile
  {
    //members public for speed purposes
      

    private char symbol = Constants.SymbolBackground;
    public string name = "";
    protected string displayedName = "";
    public string mapName;

#if DEBUG_PROPS
    public string _tag1 = "";
    public string tag1
    {
      get { return _tag1; }
      set
      {
        if (_tag1.Contains("grave"))
        {
          int k = 0;
          k++;
        }
        _tag1 = value;
        if (value == "fallen_one")
        { 
          if(point.Y == 0)
          {
            int k = 0;
            k++;
          }
        }
        if (_tag1 == "Wall1")
        {
          int k = 0;
          k++;
        }
      }
    } //custom purpose field
    public Point _point;
    public Point point
    {
      get { return _point; }
      set
      {
        if (value.X == 0 && value.Y == 0)
        {
          int k = 0;
          k++;
        }
        _point = value;
       
      }
    }

#else
    public string tag1 = "";//custom purpose field
    public Point point;
#endif

    public string tag2 = "";//custom purpose field
    public ConsoleColor color = ConsoleColor.White;

    /// <summary>
    /// The index of the node (room) the tile belongs to
    /// </summary>
    //public int DungeonNodeIndex = Constants.MinNormalNodeIndex - 1;

    /// <summary>
    /// If the tile is at node's corner this member says which corner it is.
    /// </summary>
    public TileCorner? corner;

#if DEBUG_PROPS
  bool revealed;
  public bool Revealed 
  { 
    get
    {
        return revealed;
    }
    set
    {
      if (this.DungeonNodeIndex == DungeonNode.ChildIslandNodeIndex)
      {
          if (this is Wall)
          {
            int k = 0;
            k++;  
          }
          if (this is IDoor)
          {
            int k = 0;
            k++;
          }
        }
      if(value == true)
      {
        int k = 0;
        k++;
        if (this is IDoor)
        {
          int kk = 0;
          kk++;
        }
      }
      revealed  = value;
    } 
  }
#else
    /// <summary>
    /// If false the tile is not visible. The revealed flag shall be typically set to true when a door leading to room are opened.
    /// </summary>
    public bool Revealed = GenerationInfo.DefaultRevealedValue;
#endif


    [JsonIgnore]
    public string DestroySound { get; set; }

    [JsonIgnore]
    public string GainedHitSound { get; set; }


    public Tile() : this(Constants.SymbolBackground)
    {
    }

    public Tile(char symbol = Constants.SymbolBackground) : this(GenerationConstraints.InvalidPoint, symbol)
    {

    }

    public Tile(Point point, char symbol = Constants.SymbolBackground)
    {
      this.Name = GetDefaultName();
      this.point = point;
      this.Symbol = symbol;
    }

    protected virtual string GetDefaultName()
    {
      return GetType().Name;
    }

    public string GetNameFromTag1()
    {
      return GetNameFromTag1(tag1);
    }

    public string GetNameFromTag1(string tag1)
    {
      var str = tag1.Replace("_ch", "");
      str = str.Replace("_", " ");
      return str.ToUpperFirstLetter();
    }

    public virtual void SetNameFromTag1()
    {
      DisplayedName = "";
      Name = GetNameFromTag1();
    }

    //some classes might need a prop
    public int CustomDungeonNodeIndex
    {
      get { return DungeonNodeIndex; }
      set { DungeonNodeIndex = value; }
    }
        
#if DEBUG_PROPS
    int dungeonNodeIndex = DungeonNode.DefaultNodeIndex;
    [JsonIgnore]
    public int DungeonNodeIndex
    {
      get { return dungeonNodeIndex; }
      set
      {
        if (value == 100 && Symbol == '@')
        {
          int k = 0;
          k++;
        }
        if (value == DungeonNode.ChildIslandNodeIndex)
        {
          int k = 0;
          k++;
        }
        if (dungeonNodeIndex == DungeonNode.ChildIslandNodeIndex && value != DungeonNode.ChildIslandNodeIndex)
        {
          int k = 0;
          k++;
        }

        dungeonNodeIndex = value;
      }
    }
#else
    public int DungeonNodeIndex = DungeonNode.DefaultNodeIndex;
#endif

    [JsonIgnore]
    public bool IsAtValidPoint
    {
      get { return point != GenerationConstraints.InvalidPoint; }
    }

    [JsonIgnore]
    public bool IsEmpty { get { return symbol == Constants.SymbolBackground; } }

    //[JsonIgnore] item name was lost
    public virtual string Name
    {
      get
      {
        return name;
      }
      set
      {
        name = value.Trim();//call GetCapitalized(value) ?;
        //if (Symbol != Constants.SymbolBackground)
        {
          if (DisplayedNameNeedsToBeSet())
            DisplayedName = EnsureLevelNotInAssetName(name);
        }
      }
    }

    public string EnsureLevelNotInAssetName(string asset)
    {
      if (asset.Contains("_level"))
      {
        asset = asset.Substring(0, asset.IndexOf("_level"));
      }
      else if (asset.Contains(" level"))
      {
        asset = asset.Substring(0, asset.IndexOf(" level"));
      }

      return asset;
    }

    protected virtual bool DisplayedNameNeedsToBeSet()
    {
      return string.IsNullOrEmpty(displayedName) || displayedName == "Unset" || GetDefaultName() == displayedName;
    }

    public static string GetCapitalized(string val)
    {
      return val.GetCapitalized();
    }

    public bool IsFromChildIsland
    {
      get { return DungeonNodeIndex < Constants.MinNormalNodeIndex; }
    }

    //public float RevealPercent { get; set; }

    public virtual char Symbol
    {
      get
      {
        return symbol;
      }

      set
      {
        symbol = value;
      }
    }

    [JsonIgnore]
    public ConsoleColor Color { get => color; set => color = value; }

    //[JsonIgnore]
    public virtual string DisplayedName
    {
      get
      {
        return displayedName;
      }
      set
      {
        //if (displayedName == "Bloody Mary")
        //{
        //  int k = 0;
        //  k++;
        //}
        //if(value == "Ice scepter1")
        //{
        //  int k = 0;
        //  k++;
        //}
        displayedName = value;
      }
    }

    public bool IsAtSamePosition(Tile other)
    {
      return point.Equals(other.point);
    }

#if DEBUG_PROPS
  public static bool IncludeDebugDetailsInToString = true;
#else
  public static bool IncludeDebugDetailsInToString = true;
#endif


    public override string ToString()
    {
      string res = GetType().ToString() + " " + Name + " " + tag1;
      if (IncludeDebugDetailsInToString)
        res += " " + symbol + " " + DungeonNodeIndex + " " + point + " " + tag1 + " " + GetHashCode();
      return res;
    }

    public double DistanceFrom(Tile other)
    {
      return DistanceFrom(other.point);
    }

    public double DistanceFrom(Point other)
    {
      return point.DistanceFrom(other);
    }
  }
}
