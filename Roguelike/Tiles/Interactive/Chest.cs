﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Tiles.Interactive
{
  public enum ChestKind { Unset, Plain, Gold, GoldDeluxe }

  public class Chest : InteractiveTile
  {
    public const char ChestSymbol = '~';
    private ChestKind _chestKind;

    public ChestKind ChestKind
    {
      get => _chestKind;
      set
      {
        _chestKind = value;
        if (_chestKind == ChestKind.Plain)
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

    //public bool IsGold { get { return ChestKind == ChestKind.Gold; } }

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
