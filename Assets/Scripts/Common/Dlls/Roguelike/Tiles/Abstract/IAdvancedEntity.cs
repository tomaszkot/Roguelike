using Roguelike.Abilities;
using Roguelike.Abstract.Inventory;
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
    bool IncreaseAbility(AbilityKind kind);

    bool IncreaseSpell(SpellKind sk);


    PassiveAbility GetPassiveAbility(AbilityKind kind);
    ActiveAbility GetActiveAbility(AbilityKind kind);
    string GetExpInfo();

    event EventHandler StatsRecalculated;

  }
}
