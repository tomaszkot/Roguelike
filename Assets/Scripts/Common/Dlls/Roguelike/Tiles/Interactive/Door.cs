using Dungeons;
using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Roguelike.Tiles.Interactive
{
  public enum DoorOpenerKind  { Unset, Key, Lever}

  public class Door : InteractiveTile, Dungeons.Tiles.IDoor
  {
    public Door(Container cont,Point point) : base(cont, Dungeons.Tiles.Constants.SymbolDoor)
    {
      base.point = point;
      Color = ConsoleColor.Yellow;
    }

    public Door(Container cont) : this(cont, GenerationConstraints.InvalidPoint)
    {
      tag1 = "doors_closed";
    }

    public DoorOpenerKind OpenerKind { get; set; }

    public string PitName { get; set; }

    public EntranceSide EntranceSide { get; set; }

    public string KeyName
    {
      get => keyName;
      set
      {
        keyName = value;
        if (keyName.Any())
          OrgKeyName = keyName;
      }
    }
    public string OrgKeyName { get; set; }

    public void MakeOpen()
    {
      KeyName = "";
      Opened = true;
    }
    //public void MakeClosed()
    //{
    //  KeyName = OrgKeyName;
    //  Opened = false;
    //}

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
    private string keyName = "";

    public string BossBehind
    {
      get { return bossBehind; }
      set
      {
        bossBehind = value;
        Color = ConsoleColor.Red;
      }
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

    public KeyPuzzle KeyPuzzle { get; set; }
  }
}
