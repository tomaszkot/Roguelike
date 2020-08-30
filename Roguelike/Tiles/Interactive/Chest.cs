using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Tiles.Interactive
{
  public enum ChestKind { Unset, Plain, Gold, GoldDeluxe }

  public class Chest : InteractiveTile
  {
    public const char ChestSymbol = '~';
    private ChestKind chestKind = ChestKind.Plain;

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
    public bool Closed { get; set; } = true;

    public Chest() : base(ChestSymbol)
    {
      Symbol = ChestSymbol;
      Name = "Chest";
      
      Kind = InteractiveTileKind.TreasureChest;
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
