using OuaDII.TileContainers;
using Roguelike.TileContainers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Serialization
{
  public interface IPersister : Roguelike.Serialization.IPersister
  {
    void SaveWorld(string heroName, World world, bool quick);
    World LoadWorld(string heroName, bool quick);

    void LoadWorld(string heroName, bool quick, World world);

    void SavePits(string heroName, List<DungeonPit> pits, bool quick);
    List<DungeonPit> LoadPits(string heroName, bool quick);
  }
}
