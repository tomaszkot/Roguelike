using Roguelike.LootContainers;

namespace Roguelike.Tiles.Abstract
{
  public interface IEquipable
  {
    CurrentEquipment CurrentEquipment { get; set; }
  }
}
