using Roguelike.Abilities;
using Roguelike.Abstract.Inventory;
using Roguelike.LootContainers;
using Roguelike.Spells;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;

namespace Roguelike.Tiles.Abstract
{
  public interface IAdvancedEntity : IInventoryOwner
  {
    int Level { get; }

    Dictionary<CurrentEquipmentKind, IEquipment> GetActiveEquipment();

    int AbilityPoints { get; set; }
    AbilitiesSet Abilities { get; }
    SpellStateSet Spells { get; }
    bool IsMecenary { get; }
    bool IsMercenary { get; }

    bool IncreaseAbility(AbilityKind kind);

    bool IncreaseSpell(SpellKind sk);


    PassiveAbility GetPassiveAbility(AbilityKind kind);
    ActiveAbility GetActiveAbility(AbilityKind kind);
    string GetExpInfo();
    bool InventoryAcceptsItem(Inventory inventory, Loot loot, AddItemArg addItemArg);
    bool MoveEquipmentCurrent2Inv(IEquipment eq, CurrentEquipmentKind cek);

    event EventHandler StatsRecalculated;

  }
}
