using Dungeons.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons
{
  public class HiddenTilesInfo
  {
    List<Tile> tiles { get; set; } = new List<Tile>();

    public IEnumerable<Tile> Tiles { get { return tiles; } }

    public void Add(Tile tile) { tiles.Add(tile); }
  }

  public class HiddenTiles
  {
    Dictionary<string, HiddenTilesInfo> Tiles { get; set; } = new Dictionary<string, HiddenTilesInfo>();
        
    public bool Contains(string key)
    {
      return Tiles.ContainsKey(key);
    }

    public HiddenTilesInfo Get(string key)
    {
      return Tiles[key];
    }

    public HiddenTilesInfo Ensure(string key)
    {
      if (!Tiles.ContainsKey(key))
        Tiles.Add(key, new HiddenTilesInfo());

      return Tiles[key];

    }
  } 
}
