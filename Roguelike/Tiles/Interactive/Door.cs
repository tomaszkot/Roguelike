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

    public string OpenedSound()
    {
      if (IsBigGate())
        return "GateMetalOpen";
      return "door_open";
    }

    public string ClosedSound()
    {
      if (IsBigGate())
        return "GateMetalClose";
      return "door_close";
    }

    public override string InteractSound 
    {
      get 
      {
        if (IsBigGate())
          return "GATE_Metal_Close_11.wav";


        return base.InteractSound;
      }
      set 
      {
        base.InteractSound = value;
      } 
    }
  }
}
