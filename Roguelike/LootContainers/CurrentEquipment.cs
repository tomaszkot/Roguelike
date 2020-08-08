using Roguelike.Tiles;
using Roguelike.Utils;
using System;

namespace Roguelike.LootContainers
{
  public class CurrentEquipment 
  {
    //putOnEquipment - currently worn eq. all values can be not null  
    SerializableDictionary<CurrentEquipmentKind, Equipment> primaryEquipment = new SerializableDictionary<CurrentEquipmentKind, Equipment>();

    //spareEquipment - currently only weapon/shield can be not null
    SerializableDictionary<CurrentEquipmentKind, Equipment> spareEquipment = new SerializableDictionary<CurrentEquipmentKind, Equipment>();
    
    public CurrentEquipment()
    {
      var eqipTypes = Enum.GetValues(typeof(CurrentEquipmentKind));
      foreach (CurrentEquipmentKind et in eqipTypes)
      {
        PrimaryEquipment[et] = null;
        SpareEquipment[et] = null;
        SpareEquipmentUsed[et] = false;
      }
    }

    public SerializableDictionary<CurrentEquipmentKind, Equipment> PrimaryEquipment { get => primaryEquipment; set => primaryEquipment = value; }
    public SerializableDictionary<CurrentEquipmentKind, Equipment> SpareEquipment { get => spareEquipment; set => spareEquipment = value; }

    //currently only weapon/shield can be not null
    public SerializableDictionary<CurrentEquipmentKind, bool> SpareEquipmentUsed { get; set; } = new SerializableDictionary<CurrentEquipmentKind, bool>();
  }
}
