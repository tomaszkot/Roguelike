﻿namespace Roguelike.Tiles.Looting
{
  public enum GobletKind { Silver, Gold }

  public class Goblet : StackedLoot
  {
    private GobletKind gobletKind;

    public GobletKind GobletKind
    {
      get => gobletKind;
      set
      {
        gobletKind = value;
        switch (gobletKind)
        {
          case GobletKind.Silver:
            Name = "Silver Goblet";
            PrimaryStatDescription = "Vessel woth a couple of coins";
            Price = 8;
            break;
          case GobletKind.Gold:
            Name = "Gold Goblet";
            PrimaryStatDescription = "Vessel woth a lot of coins";
            Price = 50;
            break;
          default:
            break;
        }
      }
    }

    public Goblet()
    {
      Symbol = '&';
      tag1 = "goblet";
      GobletKind = GobletKind.Silver;
    }
  }

  public class GenericLoot : StackedLoot
  {
    public string Kind { get; set; }

    public GenericLoot() : this("?", "?", "?")
    {
    }

    public GenericLoot(string kind, string description, string asset)
    {
      Symbol = '&';
      Price = 5;
      Kind = kind;
      PrimaryStatDescription = description;
      tag1 = asset;
      Name = kind;
      //LootKind = LootKind.Other;
    }

    public override string GetId()
    {
      return Kind + "_" + base.GetId();
    }
  }
}
