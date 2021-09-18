using System;
using System.Drawing;

namespace Roguelike.Tiles
{
  public interface ILootSource
  {
    //Difficulty difficulty { get; }
    int Level { get; }
    bool SetLevel(int level, Difficulty? diff = null);
    Point GetPoint();
    string OriginMap { get; set; }
  }
}

namespace Roguelike.Tiles.Interactive
{
  public enum ChestKind { Unset, Plain, Gold, GoldDeluxe }
  public enum ChestVisualKind { Unset, Chest, Grave }

  public class Chest : InteractiveTile, ILootSource
  {
    public const char ChestSymbol = '~';
    private ChestKind chestKind = ChestKind.Plain;
    
    private ChestVisualKind chestVisualKind = ChestVisualKind.Chest;

    public string OriginMap { get; set; }
    
    public event EventHandler RequiredKey;
    public bool Locked { get; set; } = false;
    public string KeyName { get; set; }//key for unlocking
    public string UnhidingMapName { get; set; }

    /// <summary>
    /// ctor!
    /// </summary>
    public Chest() : base(ChestSymbol)
    {
      tag1 = "chest_plain1";
      Symbol = ChestSymbol;
      Name = "Chest";

      Kind = InteractiveTileKind.TreasureChest;

      InteractSound = "chest_open";
    }

    public ChestVisualKind ChestVisualKind
    {
      get => chestVisualKind;
      set
      {
        chestVisualKind = value;
        if (chestVisualKind == ChestVisualKind.Grave)
          InteractSound = "grave_open";
      }
    }
    public ChestKind ChestKind
    {
      get => chestKind;
      set
      {
        chestKind = value;
        SetColor();
      }
    }

    public override void ResetToDefaults()
    {
      Closed = true;
    }

    private void SetColor()
    {
      if (Closed)
        Color = ConsoleColor.DarkGreen;
      else
      {
        if (chestKind == ChestKind.Plain)
          Color = ConsoleColor.Cyan;
        else
          Color = ConsoleColor.Yellow;
      }
    }

    public Point GetPoint() { return point; }


    public bool Closed
    {
      get => !IsLooted;
      set
      {
        IsLooted = value;
        SetColor();
      }
    }
    public bool SetLevel(int level, Difficulty? diff = null)
    {
      Level = level;
      return true;
    }

    public bool Open(string keyName = "")
    {
      if (Closed)
      {
        if (!Locked || keyName == KeyName)
        {
          SetLooted(true);
          //if (Opened != null)
          //  Opened(this, EventArgs.Empty);
          return true;
        }
        else if (RequiredKey != null)
          RequiredKey(this, EventArgs.Empty);

      }

      return false;
    }

    public LootSourceKind LootSourceKind
    {
      get
      {
        if (ChestKind == ChestKind.Plain)
          return LootSourceKind.PlainChest;
        if (ChestKind == ChestKind.Gold)
          return LootSourceKind.GoldChest;

        return LootSourceKind.DeluxeGoldChest;
      }
    }
  }
}
