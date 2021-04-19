using Roguelike.Abilities;
using Roguelike.Abstract.Inventory;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;

namespace Roguelike.Tiles.Abstract
{
  public interface IAdvancedEntity : IInventoryOwner
  {
    int Level { get; }

    Dictionary<CurrentEquipmentKind, Equipment> GetActiveEquipment();
    
    int AbilityPoints { get; set; }
    AbilitiesSet Abilities { get; }
    bool IncreaseAbility(PassiveAbilityKind kind);
    PassiveAbility GetAbility(PassiveAbilityKind kind);
    string GetExpInfo();

    event EventHandler StatsRecalculated;
  }
}
