using Roguelike.Abilities;
using Roguelike.Abstract.TileParts;
using Roguelike.Tiles;
using System.Collections.Generic;

namespace Roguelike.Abstract.Tiles
{
  public interface IAdvancedEntity : IInventoryOwner
  {
    int Level { get; }

    Dictionary<CurrentEquipmentKind, Equipment> GetActiveEquipment();
    
    int AbilityPoints { get; set; }
    Abilities.AbilitiesSet Abilities { get; }
    bool IncreaseAbility(AbilityKind kind);
    Ability GetAbility(AbilityKind kind);
  }
}
