using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Tiles
{
  public interface ILootSource
  {
    int Level { get; }
    void SetLevel(int level);
  }
}

namespace Roguelike.Tiles.Interactive
{
  public enum ChestKind { Unset, Plain, Gold, GoldDeluxe }

  

  public class Chest : InteractiveTile, ILootSource
  {
    public const char ChestSymbol = '~';
    private ChestKind chestKind = ChestKind.Plain;
    public event EventHandler Opened;

    public ChestKind ChestKind
    {
      get => chestKind;
      set
      {
        chestKind = value;
        if (chestKind == ChestKind.Plain)
          Color = ConsoleColor.Cyan;
        else
          Color = ConsoleColor.Yellow;
      }
    }

    public bool Closed
    {
      get;
      set;
    } = true;

    public void SetLevel(int level) { Level = level; }

    public Chest() : base(ChestSymbol)
    {
      Symbol = ChestSymbol;
      Name = "Chest";
      
      Kind = InteractiveTileKind.TreasureChest;
      InteractSound = "chest_open";
    }

    public bool Open()
    {
      if (Closed)
      {
        Closed = false;
        if (Opened != null)
          Opened(this, EventArgs.Empty);
        return true;
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
