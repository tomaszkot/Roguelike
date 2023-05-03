using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
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
    public IEquipment Equipment { get; set; }
    public CurrentEquipmentKind CurrentEquipmentKind { get; set; }
    public Loot Removed { get; internal set; }
  }
  public class CurrentEquipment : Inventory
  {
    public event EventHandler<EquipmentChangedArgs> EquipmentChanged;
    //putOnEquipment - currently worn eq. all values can be not null  
    SerializableDictionary<CurrentEquipmentKind, IEquipment> primaryEquipment = new SerializableDictionary<CurrentEquipmentKind, IEquipment>();

    //spareEquipment - currently only weapon/shield can be not null
    SerializableDictionary<CurrentEquipmentKind, IEquipment> spareEquipment = new SerializableDictionary<CurrentEquipmentKind, IEquipment>();

    public SerializableDictionary<CurrentEquipmentKind, IEquipment> PrimaryEquipment { get => primaryEquipment; set => primaryEquipment = value; }
    public SerializableDictionary<CurrentEquipmentKind, IEquipment> SpareEquipment { get => spareEquipment; set => spareEquipment = value; }

    //currently only weapon/shield can be not null
    public SerializableDictionary<CurrentEquipmentKind, bool> SpareEquipmentUsed { get; set; } = new SerializableDictionary<CurrentEquipmentKind, bool>();

    public bool GodActivated { get; set; }

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

    public bool HasWeapon(bool primaryEq)
    {
      if (primaryEq)
        return PrimaryEquipment[CurrentEquipmentKind.Weapon] != null;

      return SpareEquipment[CurrentEquipmentKind.Weapon] != null;
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
      return GetActiveEquipment(CurrentEquipmentKind.Weapon) as Weapon;
    }

    public Armor GetHelmet()
    {
      return GetActiveEquipment(CurrentEquipmentKind.Helmet) as Armor;
    }

    public IEquipment GetActiveEquipment(CurrentEquipmentKind cek)
    {
      return GetActiveEquipment()[cek];
    }

    public Dictionary<CurrentEquipmentKind, IEquipment> GetActiveEquipment()
    {
      var result = new Dictionary<CurrentEquipmentKind, IEquipment>();
      foreach (var pos in PrimaryEquipment)//PrimaryEquipment has all kinds
      {
        var eq = SpareEquipmentUsed[pos.Key] ? SpareEquipment[pos.Key] : PrimaryEquipment[pos.Key];
        result[pos.Key] = eq;
      }

      return result;
    }

    public override bool Add(Loot item, AddItemArg args = null)
    {
      var eq = item as IEquipment;
      if (eq == null)
        return false;

      CurrentEquipmentAddItemArg castedArgs = null;
      if (args != null)
        castedArgs = args as CurrentEquipmentAddItemArg;

      if (castedArgs == null)
        castedArgs = new CurrentEquipmentAddItemArg();

      return SetEquipment(eq, castedArgs.cek, castedArgs.primary);
    }

    public override Loot Remove(Loot loot, RemoveItemArg arg)
    {
      Loot itemToRemove = loot;
      var eq = loot as IEquipment;
      SerializableDictionary<CurrentEquipmentKind, IEquipment> equipmentSet = null;
      if (PrimaryEquipment.Any(i => i.Value == eq))
        equipmentSet = PrimaryEquipment;
      else
        equipmentSet = SpareEquipment;

      var cek = CurrentEquipmentKind.Unset;
      if (equipmentSet.Any(i => i.Value == eq))
        cek = equipmentSet.First(i => i.Value == eq).Key;

      bool setNull = true;
      if (loot is StackedLoot sl)
      {
        itemToRemove = sl.Clone(arg.StackedCount);
        var stackedItemCount = GetStackedCount(sl);
        if (stackedItemCount >= arg.StackedCount)
        {
          setNull = false;
          stackedItemCount -= arg.StackedCount;
          SetStackCount(sl, stackedItemCount);
          if (stackedItemCount <= 0)
            setNull = true;
        }
      }
      EventsManager.AppendAction(new InventoryAction(this) { Info="Loot removed: "+ loot.DisplayedName, Kind= InventoryActionKind.ItemRemoved} );
      if (cek != CurrentEquipmentKind.Unset)
      {
        if(setNull)
          SetEquipment(null, cek, equipmentSet == PrimaryEquipment, loot );
        return itemToRemove;
      }

      return null;
    }

    public bool SetEquipment(IEquipment eq, CurrentEquipmentKind cek = CurrentEquipmentKind.Unset, bool primary = true, Loot removed = null)
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

      primary = SpareEquipmentUsed[cek] ? false : true;

      SerializableDictionary<CurrentEquipmentKind, IEquipment> dict = null;
      if (primary)
        dict = PrimaryEquipment;
      else
        dict = SpareEquipment;

      bool done = false;
      if (eq != null && dict[cek] != null)
      {
        var sl = eq as StackedLoot;
        var dicSl = dict[cek] as StackedLoot;
        if (sl != null && dicSl != null)
        {
          if (sl.Name != dicSl.Name)
            return false;
          else
          {
            done = true;
            dict[cek].Count += sl.Count;
          }
        }
        else
          return false;
      }
      if(!done)
        dict[cek] = eq;

      if (EquipmentChanged != null)
        EquipmentChanged(this, new EquipmentChangedArgs() { Equipment = eq, CurrentEquipmentKind = cek, Removed = removed });

      return true;
    }

    public static bool EnsureCurrEqKind(IEquipment eq, ref CurrentEquipmentKind cek)
    {
      if (cek == CurrentEquipmentKind.Unset)
      {
        if (eq == null)
          return false;
        cek = eq.EquipmentKind.GetCurrentEquipmentKind(CurrentEquipmentPosition.Left);
      }

      return true;
    }

    protected override StackedLoot GetStackedItem(Loot loot)
    {
      var sl = PrimaryEquipment.Values.FirstOrDefault(i => i as Loot == loot) as StackedLoot;
      if (sl == null)
        sl = SpareEquipment.Values.FirstOrDefault(i => i as Loot == loot) as StackedLoot;
      return sl;
    }
  }
}
