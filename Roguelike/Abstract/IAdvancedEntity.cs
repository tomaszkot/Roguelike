using Roguelike.Tiles;
using System.Collections.Generic;

namespace Roguelike.Abstract
{
  public interface IAdvancedEntity : IInventoryOwner
  {
    int Level { get; }

    Dictionary<CurrentEquipmentKind, Equipment> GetActiveEquipment();
    
    int AbilityPoints { get; set; }
    Abilities.AbilitiesSet Abilities { get; }
  }
}
