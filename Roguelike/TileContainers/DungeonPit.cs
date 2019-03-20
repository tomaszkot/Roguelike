using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.TileContainers
{
  //set of levels, enterance is from a World
  public class DungeonPit
  {
    //List<DungeonLevel> levels;
    public string Name { get; set; }
    public List<DungeonLevel> Levels { get; set; } = new List<DungeonLevel>();

    public void AddLevel(DungeonLevel lvl)
    {
      lvl.PitName = Name;
      Levels.Add(lvl);
    }
  }
}
