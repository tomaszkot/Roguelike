using Newtonsoft.Json;
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
  }
}
