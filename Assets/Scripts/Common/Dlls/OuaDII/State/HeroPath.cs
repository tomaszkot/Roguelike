using OuaDII.TileContainers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.State
{
  public class HeroPath : Roguelike.State.HeroPath
  {
    public override string GetDisplayName()
    {
      string name = "";
      if (Pit.Any())
        name += DungeonPit.GetPitDisplayName(Pit) + "/" + (LevelIndex + 1);
      else
        name += World;

      return name;
    }


  }
}
