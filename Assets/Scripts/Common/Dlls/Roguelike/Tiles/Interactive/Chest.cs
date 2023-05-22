using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Tiles.Abstract;
using SimpleInjector;
using System;
using System.Drawing;

namespace Roguelike.Tiles.Interactive
{
  public enum ChestKind { Unset, Plain, Gold, GoldDeluxe }
  public enum ChestVisualKind { Unset, Chest, Grave }

  public class Chest : InteractiveTile, ILootSource, IDestroyable
  {
    public const char ChestSymbol = '~';
    private ChestKind chestKind = ChestKind.Plain;

    private ChestVisualKind chestVisualKind = ChestVisualKind.Chest;

    [JsonIgnore]
    public bool RewardGenerated { get; set; }
    public string OriginMap { get; set; }

    public event EventHandler RequiredKey;
    public bool Locked { get; set; } = false;
    public string KeyName { get; set; }//key for unlocking
    public string UnhidingMapName { get; set; }

    /// <summary>
    /// ctor!
    /// </summary>
    public Chest(Container cont) : base(cont, ChestSymbol)
    {
      tag1 = "chest_plain1";
      Symbol = ChestSymbol;
      Name = "Chest";

      Kind = InteractiveTileKind.TreasureChest;

      InteractSound = "chest_open";
      ChestKind = ChestKind.Plain;
      DestroySound = "chest_broken";
    }

    public bool LevelSet { get; set; }
    public Loot ForcedReward { get; set; }
    public ChestVisualKind ChestVisualKind
    {
      get => chestVisualKind;
      set
      {
        chestVisualKind = value;
        if (chestVisualKind == ChestVisualKind.Grave)
        {
          InteractSound = "grave_open";
          tag1 = RandHelper.GetRandomDouble() < 0.5 ? "grave_full2" : "grave_full1";
        }
      }
    }
    public ChestKind ChestKind
    {
      get => chestKind;
      set
      {
        chestKind = value;
        SetColor();
        SetTag1BasedOnKind();
      }
    }

    void SetTag1BasedOnKind()
    {
      tag1 = "chest_plain1";
      if (ChestKind == ChestKind.Gold)
        tag1 = "chest_gold";
      else if (ChestKind == ChestKind.GoldDeluxe)
        tag1 = "chest_gold_deluxe";
    }

    public override void ResetToDefaults()
    {
      Closed = true;
    }

    private void SetColor()
    {
      if (Closed)
      {
        Color = ConsoleColor.DarkGreen;
        if (chestKind != ChestKind.Plain)
          Color = ConsoleColor.DarkYellow;
      }
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
        IsLooted = !value;
        SetColor();
      }
    }
    public bool SetLevel(int level, Difficulty? diff = null)
    {
      Level = level;
      LevelSet = true;
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
      else 
      {
        RegistedHitWhenOpened();
      }

      return false;
    }

    internal void RegistedHitWhenOpened()
    {
      Durability--;
      if (Durability == 0 && !Destroyed)
        Destroyed = true;
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

    public bool CanBeDestroyed { get { return !Closed && ChestVisualKind != ChestVisualKind.Grave; }  }

    public int Durability { get; set; } = 3;

    public bool Destroyed { get; set; }
  }
}
