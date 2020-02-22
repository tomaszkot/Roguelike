using Roguelike.Tiles;
using Roguelike.Utils;
using System;

namespace Roguelike.TileParts
{
  public class CurrentEquipment 
  {
    //putOnEquipment - currently worn eq. all values can be not null  
    SerializableDictionary<EquipmentKind, Equipment> primaryEquipment = new SerializableDictionary<EquipmentKind, Equipment>();

    //spareEquipment - currently only weapon/shield can be not null
    SerializableDictionary<EquipmentKind, Equipment> spareEquipment = new SerializableDictionary<EquipmentKind, Equipment>();
    
    public CurrentEquipment()
    {
      var eqipTypes = Enum.GetValues(typeof(EquipmentKind));
      foreach (EquipmentKind et in eqipTypes)
      {
        PrimaryEquipment[et] = null;
        SpareEquipment[et] = null;
        SpareEquipmentUsed[et] = false;
      }
    }

    public SerializableDictionary<EquipmentKind, Equipment> PrimaryEquipment { get => primaryEquipment; set => primaryEquipment = value; }
    public SerializableDictionary<EquipmentKind, Equipment> SpareEquipment { get => spareEquipment; set => spareEquipment = value; }

    //currently only weapon/shield can be not null
    public SerializableDictionary<EquipmentKind, bool> SpareEquipmentUsed { get; set; } = new SerializableDictionary<EquipmentKind, bool>();
  }
}
