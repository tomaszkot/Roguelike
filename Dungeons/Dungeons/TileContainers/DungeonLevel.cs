using SimpleInjector;

namespace Dungeons.TileContainers
{
  public interface IDungeonLevel { }

  //result of merging  of many DungeonNodes 
  public class DungeonLevel : DungeonNode, IDungeonLevel
  {
    public DungeonLevel(Container container) : base(container)
    {

    }
  }
}
