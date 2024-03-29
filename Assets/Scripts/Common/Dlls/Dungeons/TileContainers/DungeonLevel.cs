﻿using Dungeons.Core;
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
        var alreadyAtPos = GetTile(unityTile.point);
        if (alreadyAtPos != null && selector(alreadyAtPos))
        {
          logger.LogInfo("alreadyAtPos " + alreadyAtPos);
          //find new empty place 
          var empty = GetClosestEmpty(alreadyAtPos, false, skip);
          int counter = 0;
          while (empty != null)
          {
            counter++;
            if (counter == 200)
            {
              logger.LogError("endless loop!");
              return false;
            }
            var srcTile = src.FirstOrDefault(i => i.point == empty.point);
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
          var set = SetTile(alreadyAtPos, empty.point);
          if (!set)
          {
            logger.LogError("!set alreadyAtPos " + alreadyAtPos);
            return false;
          }
          var check = GetTile(empty.point);
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
      get
      {
        if (!Parts.Any())
          return new List<DungeonNode>();
        return Parts[0].Parts.ToList();
      }
    }

    [JsonIgnore]
    public int SecretRoomIndex { get; internal set; } = -1;

    public bool Merge(List<Tile> src, Point point, Func<Tile, bool> rearrangeSelector)
    {
      var set = Rearrange(src, rearrangeSelector);
      if (!set)
        return false;
      foreach (var tile in src)
      {
        set &= this.SetTile(tile, tile.point);
      }

      return set;
    }
  }
}
