using Roguelike.Tiles;
using Roguelike.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.TileParts
{
  class CurrentInventory //TODO rename to CurrentEquipment
  {
    SerializableDictionary<EquipmentKind, Equipment> primaryEquipment = new SerializableDictionary<EquipmentKind, Equipment>();
    SerializableDictionary<EquipmentKind, Equipment> secondaryEquipment = new SerializableDictionary<EquipmentKind, Equipment>();

    public CurrentInventory()
    {
      var eqipTypes = Enum.GetValues(typeof(EquipmentKind));
      foreach (EquipmentKind et in eqipTypes)
      {
        PrimaryEquipment[et] = null;
        SecondaryEquipment[et] = null;
      }
    }

    public SerializableDictionary<EquipmentKind, Equipment> PrimaryEquipment { get => primaryEquipment; set => primaryEquipment = value; }
    public SerializableDictionary<EquipmentKind, Equipment> SecondaryEquipment { get => secondaryEquipment; set => secondaryEquipment = value; }
  }
}
