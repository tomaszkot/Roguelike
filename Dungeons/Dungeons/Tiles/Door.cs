﻿using Dungeons.Core;
using System;
using System.Drawing;

namespace Dungeons.Tiles
{
  public interface IDoor
  {
    bool Opened
    {
      get;
      set;
    }
  }

  class Door : InteractiveTile, IDoor
  {
    public Door(Point point) : base(point, Constants.SymbolDoor)
    {
      Color = ConsoleColor.Yellow;
    }

    public Door() : this(GenerationConstraints.InvalidPoint)
    {

    }

    public bool Opened
    {
      get;
      set;
    }
  }
}
