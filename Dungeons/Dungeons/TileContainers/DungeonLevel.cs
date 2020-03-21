using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Dungeons.TileContainers
{
  public interface IDungeonLevel { }

  //result of merging  of many DungeonNodes 
  public class DungeonLevel : DungeonNode, IDungeonLevel
  {
    public DungeonLevel(Container container) : base(container)
    {

    }

    public bool Rearrange(List<Tile> src, Func<Tile, bool> selector)
    {      
      var logger = this.Container.GetInstance<ILogger>();

      List<Tile> skip = new List<Tile>();
      foreach (var unityTile in src)
      {
        var alreadyAtPos = GetTile(unityTile.Point);
        if (alreadyAtPos != null && selector(alreadyAtPos))
        {
          logger.LogInfo("alreadyAtPos " + alreadyAtPos);
          var empty = GetClosestEmpty(alreadyAtPos, false, skip);
          while (empty!=null)
          {
            var srcTile = src.FirstOrDefault(i=> i.Point == empty.Point);
            if (srcTile != null && !srcTile.IsEmpty)
            {
              skip.Add(empty);
              empty = GetClosestEmpty(alreadyAtPos, false, skip);
            }
            else
              break;
          }
          if (empty == null)
          {
            logger.LogError("empty == null");
            return false;
          }
          var set = SetTile(alreadyAtPos, empty.Point);
          if (!set)
          {
            logger.LogError("!set alreadyAtPos " + alreadyAtPos);
            return false;
          }
          var check = GetTile(empty.Point);
          if (check != alreadyAtPos)
          {
            logger.LogError("!check != alreadyAtPos");
            return false;
          }
        }
      }

      return true;
    }

    [JsonIgnore]
    public virtual List<DungeonNode> Nodes
    {
      get { return Parts[0].Parts.ToList(); }
    }

    public void Merge(List<Tile> src, Point point, Func<Tile, bool> rearrangeSelector)
    {
      var set = Rearrange(src, rearrangeSelector);
      
      foreach (var tile in src)
      {
        this.SetTile(tile, tile.Point);
      }
    }
  }
}
