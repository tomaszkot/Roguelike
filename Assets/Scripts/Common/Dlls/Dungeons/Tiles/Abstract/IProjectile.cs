﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons.Tiles.Abstract
{
  public interface IProjectile
  {
    [JsonIgnore]
    Dungeons.Tiles.Tile Target { get; set; }

    string HitSound { get; }

    int Range 
    { 
      get; 
      set; 
    }
    bool DiesOnHit { get; set; }
    int MaxVictimsCount { get; set; }
    int Count { get; set; }
    bool MissedTarget { get; set; }

  }
}
