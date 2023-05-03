using Dungeons.Tiles;
using Roguelike.TileContainers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Roguelike.TileContainers
{
  public enum KnownLayer { Unset, Smoke }
  public class Layer
  {
    public string Name { get; set; }
    public Dictionary<Point, Tile> Tiles { get; set; } = new Dictionary<Point, Tile>();

    public bool Remove(Tile tile)
    {
      if (Tiles.ContainsKey(tile.point))
      {
        Tiles.Remove(tile.point);
        return true;
      }

      return false;
    }
  }

  public class Layers
  {
    public List<Layer> ExtraLayers { get; set; } = new List<Layer>();

    public void SetAt(string layerName, Tile tile, Point pt)
    {
      var layer = GetLayer(layerName);
      tile.point = pt;
      layer.Tiles[pt] = tile;
    }

    private Layer GetLayer(string layerName)
    {
      return ExtraLayers.Where(i => i.Name == layerName).SingleOrDefault();
    }

    public Layer GetLayer(KnownLayer knownLayer)
    {
      return GetLayer(knownLayer.ToString());
    }
    public List<T> GetTypedLayerTiles<T>(KnownLayer knownLayer) where T : Tile
    {
      return GetLayer(knownLayer.ToString()).Tiles.Values.Cast<T>().ToList();
    }

    public void SetAt(KnownLayer layer, Tile tile, Point pt)
    {
      SetAt(layer.ToString(), tile, pt);
    }

    public void SetAt<T>(KnownLayer layer, T tile, Point pt) where T : Tile
    {
      SetAt(layer.ToString(), tile, pt);
    }

    public bool ContainsAt(KnownLayer knownLayer, Point pt)
    {
      var layer = GetLayer(knownLayer);
      if (layer == null)
        return false;
      return layer.Tiles.ContainsKey(pt);
    }

    internal void Add(KnownLayer smoke)
    {
      ExtraLayers.Add(new Layer() { Name = smoke.ToString() }); ;
    }
  }
}
