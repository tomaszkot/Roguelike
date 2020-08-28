using Roguelike.Tiles;
using Roguelike.Utils;
using System;
using System.Collections.Generic;

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

    public Dictionary<CurrentEquipmentKind, Equipment> GetActiveEquipment()
    {
      var result = new Dictionary<CurrentEquipmentKind, Equipment>();
      foreach (var pos in PrimaryEquipment)//PrimaryEquipment has all kinds
      {
        var eq = SpareEquipmentUsed[pos.Key] ? SpareEquipment[pos.Key] : PrimaryEquipment[pos.Key];
        result[pos.Key] = eq;
      }

      return result;
    }
        
    public bool SetEquipment(CurrentEquipmentKind kind, Equipment eq, bool primary = true)
    {
      if (eq != null)
      {
        CurrentEquipmentPosition pos1;
        var ek = Equipment.FromCurrentEquipmentKind(kind, out pos1);
        var matches = ek == eq.EquipmentKind;
        if (!matches)
          return false;//TODO action
      }

      if (primary)
        PrimaryEquipment[kind] = eq;
      else
        SpareEquipment[kind] = eq;

      return true;
    }
    }
}
