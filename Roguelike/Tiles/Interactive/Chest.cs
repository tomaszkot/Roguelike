using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Tiles.Interactive
{
  public enum ChestKind { Plain, Gold }

  public class Chest : InteractiveTile
  {
    public const char ChestSymbol = '~';
    public ChestKind ChestKind { get; set; }

    public Chest() : base(ChestSymbol)
    {
      Symbol = ChestSymbol;
      Name = "Chest";
      Kind = InteractiveTileKind.TreasureChest;

    }

    public bool IsGold { get { return ChestKind == ChestKind.Gold; } }

//    bool generateUniq;
//    public bool GenerateUniq
//    {
//      get { return generateUniq; }
//      set
//      {
//        generateUniq = value;
//        if (value && ChestKind == ChestKind.Plain)
//          ChestKind = ChestKind.Gold;
//#if ASCII_BUILD
//      color = ConsoleColor.Yellow;
//#endif
//      }
    //}
  }
}
