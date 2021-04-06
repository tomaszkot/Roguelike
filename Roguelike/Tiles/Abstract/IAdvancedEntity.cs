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
    AbilitiesSet Abilities { get; }
    bool IncreaseAbility(PassiveAbilityKind kind);
    PassiveAbility GetAbility(PassiveAbilityKind kind);
    bool GetGoldWhenSellingTo(IAdvancedEntity dest);
  }
}
