using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Roguelike.TileContainers
{
  public class SurfaceSet
  {
    public SurfaceKind Kind { get; set; }
    public Dictionary<Point, Surface> Tiles { get; set; } = new Dictionary<Point, Surface>();

    public bool Remove(Surface tile)
    {
      if (Tiles.ContainsKey(tile.point))
      {
        Tiles.Remove(tile.point);
        return true;
      }

      return false;
    }

    public Surface GetAt(Point pt)
    {
      if (Tiles.ContainsKey(pt))
        return Tiles[pt];

      return null;
    }
  }

  public class Surfaces
  {
    public Dictionary<SurfaceKind, SurfaceSet> Sets { get; set; } = new Dictionary<SurfaceKind, SurfaceSet>();

    public Surfaces()
    {
      var kinds = EnumHelper.Values<SurfaceKind>(true);
      foreach(var kind in kinds)
        Sets[kind] = new SurfaceSet();
    }

    public void SetAt(Point pt, Surface surface)
    {
      var set = Sets[surface.Kind];
      set.Tiles[pt] = surface;
    }

    public List<Surface> GetAt(Point pt)
    {
      var surs = new List<Surface>();
      foreach (var set in Sets)
      {
        var at = set.Value.GetAt(pt);
        if (at != null)
          surs.Add(at);
      }
      return surs;
    }


    public SurfaceSet GetKind(SurfaceKind sk)
    {
      return Sets[sk];
    }

  }
}
