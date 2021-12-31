using Roguelike.Extensions;
using Roguelike.Tiles;
using Roguelike.Utils;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.LootContainers
{
  public enum ActiveWeaponSet { Unset, Primary, Secondary };

  public class EquipmentChangedArgs
  {
    public Equipment Equipment { get; set; }
    public CurrentEquipmentKind CurrentEquipmentKind { get; set; }
  };

  public class CurrentEquipment : Inventory
  {
    public event EventHandler<EquipmentChangedArgs> EquipmentChanged;
    //putOnEquipment - currently worn eq. all values can be not null  
    SerializableDictionary<CurrentEquipmentKind, Equipment> primaryEquipment = new SerializableDictionary<CurrentEquipmentKind, Equipment>();

    //spareEquipment - currently only weapon/shield can be not null
    SerializableDictionary<CurrentEquipmentKind, Equipment> spareEquipment = new SerializableDictionary<CurrentEquipmentKind, Equipment>();

    public SerializableDictionary<CurrentEquipmentKind, Equipment> PrimaryEquipment { get => primaryEquipment; set => primaryEquipment = value; }
    public SerializableDictionary<CurrentEquipmentKind, Equipment> SpareEquipment { get => spareEquipment; set => spareEquipment = value; }

    //currently only weapon/shield can be not null
    public SerializableDictionary<CurrentEquipmentKind, bool> SpareEquipmentUsed { get; set; } = new SerializableDictionary<CurrentEquipmentKind, bool>();

    public CurrentEquipment(Container container) : base(container)
    {
      var eqipTypes = Enum.GetValues(typeof(CurrentEquipmentKind));
      foreach (CurrentEquipmentKind cek in eqipTypes)
      {
        PrimaryEquipment[cek] = null;
        SpareEquipment[cek] = null;
        SpareEquipmentUsed[cek] = false;
      }
    }

    public ActiveWeaponSet GetActiveWeaponSet()
    {
      return SpareEquipmentUsed[CurrentEquipmentKind.Weapon] == true ? ActiveWeaponSet.Secondary : ActiveWeaponSet.Primary;
    }

    public ActiveWeaponSet SwapActiveWeaponSet()
    {
      if (GetActiveWeaponSet() == ActiveWeaponSet.Primary)
      {
        SpareEquipmentUsed[CurrentEquipmentKind.Weapon] = true;
        SpareEquipmentUsed[CurrentEquipmentKind.Shield] = true;
      }
      else
      {
        SpareEquipmentUsed[CurrentEquipmentKind.Weapon] = false;
        SpareEquipmentUsed[CurrentEquipmentKind.Shield] = false;
      }

      return GetActiveWeaponSet();
    }

    public Weapon GetWeapon()
    {
      var ae = GetActiveEquipment();
      return ae[CurrentEquipmentKind.Weapon] as Weapon;
    }

    public Armor GetHelmet()
    {
      var ae = GetActiveEquipment();
      return ae[CurrentEquipmentKind.Helmet] as Armor;
    }

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

    public override bool Add(Loot item, AddItemArg args = null)
    {
      var eq = item as Equipment;
      if (eq == null)
        return false;

      CurrentEquipmentAddItemArg castedArgs = null;
      if (args != null)
        castedArgs = args as CurrentEquipmentAddItemArg;

      if (castedArgs == null)
        castedArgs = new CurrentEquipmentAddItemArg();

      return SetEquipment(eq, castedArgs.cek, castedArgs.primary);
    }

    public override bool Remove(Loot loot, RemoveItemArg arg)
    {
      var eq = loot as Equipment;
      SerializableDictionary<CurrentEquipmentKind, Equipment> equipmentSet = null;
      if (PrimaryEquipment.Any(i => i.Value == eq))
        equipmentSet = PrimaryEquipment;
      else
        equipmentSet = SpareEquipment;

      var cek = CurrentEquipmentKind.Unset;
      if (equipmentSet.Any(i => i.Value == eq))
        cek = equipmentSet.First(i => i.Value == eq).Key;
      if (cek != CurrentEquipmentKind.Unset)
        return SetEquipment(null, cek, equipmentSet == PrimaryEquipment);

      return false;
    }

    public bool SetEquipment(Equipment eq, CurrentEquipmentKind cek = CurrentEquipmentKind.Unset, bool primary = true)
    {
      if (!EnsureCurrEqKind(eq, ref cek))
        return false;
      if (eq != null)
      {
        var ek = cek.GetEquipmentKind();
        var matches = ek == eq.EquipmentKind;
        if (!matches)
          return false;
      }

      if (primary)
      {
        if (eq != null && PrimaryEquipment[cek] != null)
          return false;
        PrimaryEquipment[cek] = eq;
      }
      else
      {
        if (eq != null && SpareEquipment[cek] != null)
          return false;
        SpareEquipment[cek] = eq;
      }
      if (EquipmentChanged != null)
        EquipmentChanged(this, new EquipmentChangedArgs() { Equipment = eq, CurrentEquipmentKind = cek });

      return true;
    }

    public static bool EnsureCurrEqKind(Equipment eq, ref CurrentEquipmentKind cek)
    {
      if (cek == CurrentEquipmentKind.Unset)
      {
        if (eq == null)
          return false;
        cek = eq.EquipmentKind.GetCurrentEquipmentKind(CurrentEquipmentPosition.Left);
      }

      return true;
    }
  }
}
