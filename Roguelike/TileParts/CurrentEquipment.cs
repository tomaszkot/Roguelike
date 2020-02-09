using Roguelike.Tiles;
using Roguelike.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.TileParts
{
  public class CurrentEquipment 
  {
    //putOnEquipment - currently worn eq. all values can be not null  
    SerializableDictionary<EquipmentKind, Equipment> putOnEquipment = new SerializableDictionary<EquipmentKind, Equipment>();

    //spareEquipment - currnetly only weapon/shield can be not null
    SerializableDictionary<EquipmentKind, Equipment> spareEquipment = new SerializableDictionary<EquipmentKind, Equipment>();

    public CurrentEquipment()
    {
      var eqipTypes = Enum.GetValues(typeof(EquipmentKind));
      foreach (EquipmentKind et in eqipTypes)
      {
        PutOnEquipment[et] = null;
        SpareEquipment[et] = null;
      }
    }

    public SerializableDictionary<EquipmentKind, Equipment> PutOnEquipment { get => putOnEquipment; set => putOnEquipment = value; }
    public SerializableDictionary<EquipmentKind, Equipment> SpareEquipment { get => spareEquipment; set => spareEquipment = value; }
  }
}
