using Roguelike.Tiles;
using System.Collections.Generic;

namespace Roguelike.Abstract
{
  public interface IAdvancedEntity : IInventoryOwner
  {
    Dictionary<CurrentEquipmentKind, Equipment> GetActiveEquipment();
  }
}
