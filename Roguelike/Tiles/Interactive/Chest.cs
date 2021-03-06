﻿using System;
using System.Drawing;

namespace Roguelike.Tiles
{
  public interface ILootSource
  {
    int Level { get; }
    bool SetLevel(int level);
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
    private bool closed = true;
    private ChestVisualKind chestVisualKind = ChestVisualKind.Chest;

    public string OriginMap { get; set; }
    public event EventHandler Opened;
    public event EventHandler RequiredKey;
    public bool Locked { get; set; } = false;
    public string KeyName { get; set; }//key for unlocking
    public string UnhidingMapName { get; set; }

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
      get => closed;
      set
      {
        closed = value;
        SetColor();
      }
    }
    public bool SetLevel(int level)
    {
      Level = level;
      return true;
    }

    public Chest() : base(ChestSymbol)
    {
      Symbol = ChestSymbol;
      Name = "Chest";

      Kind = InteractiveTileKind.TreasureChest;

      InteractSound = "chest_open";
    }

    public bool Open(string keyName = "")
    {
      if (Closed)
      {
        if (!Locked || keyName == KeyName)
        {
          Closed = false;
          if (Opened != null)
            Opened(this, EventArgs.Empty);
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
