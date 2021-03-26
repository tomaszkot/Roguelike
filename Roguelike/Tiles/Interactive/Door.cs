﻿using Dungeons.Core;
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

    [JsonIgnore]
    public List<Door> AllInSet { get; set; } = new List<Door>();

    string bossBehind = "";
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
      Point = point;
      Color = ConsoleColor.Yellow;
    }

    public Door() : this(GenerationConstraints.InvalidPoint)
    {

    }
  }
}