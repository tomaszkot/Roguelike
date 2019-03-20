﻿using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
  public class GameState
  {
    public class HeroPath
    {
      public string World { get; set; }
      public string Pit { get; set; } = "";
      public int LevelIndex { get; set; }

      public override string ToString()
      {
        return Pit.Any() ? this.World + " " + Pit + " " + LevelIndex  : World;
      }
    }

    public DateTime LastSaved { get; set; }
    public HeroPath HeroPathValue { get; set; } = new HeroPath();

    public override string ToString()
    {
      return HeroPathValue.ToString();
    }

  }
}
