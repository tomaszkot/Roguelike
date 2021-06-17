using Dungeons.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Roguelike.Tiles.Interactive
{
  public class Door : InteractiveTile, Dungeons.Tiles.IDoor
  {
    public string KeyName { get; set; } = "";

    public bool Opened
    {
      get;
      set;
    }

    public bool Secret
    {
      get => secret;
      set
      {
        secret = value;
        Color = ConsoleColor.Blue;
      }
    }

    [JsonIgnore]
    public List<Door> AllInSet { get; set; } = new List<Door>();

    string bossBehind = "";
    private bool secret;

    public string BossBehind
    {
      get { return bossBehind; }
      set
      {
        bossBehind = value;
        Color = ConsoleColor.Red;
      }
    }

    public Door(Point point) : base(Dungeons.Tiles.Constants.SymbolDoor)
    {
      base.point = point;
      Color = ConsoleColor.Yellow;
    }

    public Door() : this(GenerationConstraints.InvalidPoint)
    {

    }

    public bool IsBigGate() 
    {
      return tag1.Contains("gate");
    }
  }
}
