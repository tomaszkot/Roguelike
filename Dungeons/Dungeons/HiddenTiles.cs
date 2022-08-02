using Dungeons.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons
{
  /// <summary>
  /// 
  /// </summary>
  public class HiddenTilesInfo 
  {
    public List<Tile> Tiles { get; set; } = new List<Tile>();

    //public IEnumerable<Tile> Tiles { get { return Tiles; } }

    public void Add(Tile tile) { Tiles.Add(tile); }
  }

  public class HiddenTiles
  {
    public string _Name { get; set; }
    public Dictionary<string, HiddenTilesInfo> Tiles { get; set; } = new Dictionary<string, HiddenTilesInfo>();
        
    public bool Contains(string key)
    {
      return Tiles.ContainsKey(key);
    }

    public IEnumerable<String> GetKeys()
    {
      return Tiles.Keys;
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
