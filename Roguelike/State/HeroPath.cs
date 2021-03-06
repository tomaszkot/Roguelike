﻿using System.Linq;

namespace Roguelike.State
{
  public class HeroPath
  {
    public string World { get; set; }
    public string Pit { get; set; } = "";
    public int LevelIndex { get; set; }

    public override string ToString()
    {
      return GetDisplayName();
    }

    public virtual string GetDisplayName()
    {
      string name = "";
      if (Pit.Any())
        name += Pit + "/" + (LevelIndex + 1);
      else
        name += World;

      return name;
    }
  }
}
