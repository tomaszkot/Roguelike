using Newtonsoft.Json;
using SimpleInjector;
using System.Collections.Generic;
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

    [JsonIgnore]
    public virtual List<DungeonNode> Nodes
    {
      get { return Parts[0].Parts.ToList(); }
    }
  }
}
