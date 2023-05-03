using Newtonsoft.Json;
using OuaDII.LootContainers;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelike.Abstract.HotBar;
using Roguelike.Abilities;
using Roguelike.Tiles.LivingEntities;
using Dungeons.Core;

namespace OuaDII.LootContainers
{
  public enum FightItemToAbilityMapping { Unset, AbilityToFightItem }
  public class ShortcutsBar
  {
    FightItemToAbilityMapping fightItemToAbilityMapping = FightItemToAbilityMapping.AbilityToFightItem;
    private EventsManager eventsManager;
    ShortcutsBarContainer itemsContainer = new ShortcutsBarContainer();
    static int[] supportedKeys = { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48 };
    Container container;

    [JsonIgnore]
    public AdvancedLivingEntity Owner { get; set; }

    [JsonIgnore]
    Dictionary<FightItemKind, ProjectileFightItem> projSearch = new Dictionary<FightItemKind, ProjectileFightItem>();

    private void InitFightItems()
    {
      foreach (var enumVal in EnumHelper.Values<FightItemKind>(true))
      {
        projSearch[enumVal] = new ProjectileFightItem() { FightItemKind = enumVal };
      }
    }

    public ShortcutsBar(Container container)
    {
      this.container = container;
      this.EventsManager = container.GetInstance<EventsManager>();
      InitFightItems();
    }

    public static int[] GetSupportedKeys() { return supportedKeys; }

    public static bool IsKeySupported(int keyAsciiCode)
    {
      return supportedKeys.Contains(keyAsciiCode);
    }

    public static bool IsDigitSupported(int digit)
    {
      return IsKeySupported(digit + 48);
    }

    public static int GetDigit(int keyAsciiCode)
    {
      return keyAsciiCode - 48;
    }

    public bool SetAt(int digit, IHotbarItem hotbarItem)
    {
      var ok = IsDigitSupported(digit);
      if (!ok)
        return false;

      if (hotbarItem is ActiveAbility ab && 
        fightItemToAbilityMapping == FightItemToAbilityMapping.AbilityToFightItem//always true
        )
      {
        var fik = ActiveAbility.GetFightItemKind(ab.Kind);
        if (fik != FightItemKind.Unset)
        {
          var fi = GetFightItem(fik, Owner);

          if (fi == null)
          {
            projSearch[fik].Count = 0;
            hotbarItem = projSearch[fik];
          }
          else
            hotbarItem = fi;
        }
      }

      var currentDigit = GetItemDigit(hotbarItem);
      if (digit == currentDigit)
        return true;

      //remove from old position...
      bool activate = false;
      if (IsDigitSupported(currentDigit))
      {
        itemsContainer.CurrentLine.SetAt(currentDigit, null);
        EventsManager.AppendAction(new ShortcutsBarAction() { Kind = ShortcutsBarActionKind.ShortcutsBarChanged, Digit = currentDigit });
        activate = true;
      }
      //assign to the new position
      itemsContainer.CurrentLine.SetAt(digit, hotbarItem);
      EventsManager.AppendAction(new ShortcutsBarAction() { Kind = ShortcutsBarActionKind.ShortcutsBarChanged, Digit = digit });

      bool sendEv = false;
      if (ActiveItemDigit == digit)
      {
        if (hotbarItem == null)
        {
          ActiveItemDigit = -1;
          sendEv = true;
        }
      }
      else if ((activate || ActiveItemDigit == -1) && ShallAutoSelectItem(hotbarItem))
      {
        ActiveItemDigit = digit;
        sendEv = true;
      }

      if (sendEv)
        EventsManager.AppendAction(new ShortcutsBarAction() { Kind = ShortcutsBarActionKind.ActiveItemDigitChanged, Digit = digit });

      return true;
    }

    Func<IHotbarItem, bool> autoSelectItem;
    
    public bool ShallAutoSelectItem(IHotbarItem hotbarItem)
    {
      if (hotbarItem == null)
        return false;
      bool canSelectItem = IsItemSelectable(hotbarItem);
      if (canSelectItem)
      {
        if (autoSelectItem != null)
          canSelectItem = autoSelectItem(hotbarItem);//some weapons have hard logic here

        if (canSelectItem)
        {
          if (hotbarItem is Scroll sc)
          {
            canSelectItem = sc.Kind == Roguelike.Spells.SpellKind.FireBall ||
              sc.Kind == Roguelike.Spells.SpellKind.IceBall ||
              sc.Kind == Roguelike.Spells.SpellKind.PoisonBall ||
              sc.Kind == Roguelike.Spells.SpellKind.Skeleton;
          }
        }
      }
      return canSelectItem;
    }

    public void Refresh()
    {
      var keys = GetSupportedKeys();
      foreach (var key in keys)
      {
        var digit = GetDigit(key);
        var itemAt = GetAt(digit);
        if (itemAt != null)
        {
          if (!HasItem(itemAt))//stacked might have benn set to 0
            SetAt(digit, null);
        }
      }
    }

    public void AddItem(IHotbarItem item)
    {
      Refresh();

      if (HasItem(item))
      {
        var digit = GetItemDigit(item);
        EventsManager.AppendAction(new ShortcutsBarAction() { Kind = ShortcutsBarActionKind.ShortcutsBarChanged, Digit = digit });
        return;
      }

      SetAtFirstFreeSlot(item);
    }
    public StackedLoot GetStackedAt(int digit)
    {
      return GetAt(digit) as StackedLoot;
    }


    public IHotbarItem GetAt(int digit)
    {
      var item = itemsContainer.CurrentLine.GetAt(digit);
      if (fightItemToAbilityMapping == FightItemToAbilityMapping.AbilityToFightItem)
      {
        if (item is ProjectileFightItem pfi)
        {
          item = GetFightItem(pfi.FightItemKind, Owner);
          if(pfi.FightItemKind == FightItemKind.ThrowingTorch)
            item = pfi.Clone(pfi.Count);
        }
      }
      //if (item is StackedLoot sl)
      //{
      //  item = sl.Clone(sl.Count);
      //}
      return item;
    }

    public int Count
    {
      get { return itemsContainer.CurrentLine.Count; }
    }

    [JsonIgnoreAttribute]
    public ShortcutsBarContainerLine CurrentLine { get => itemsContainer.CurrentLine; set => itemsContainer.CurrentLine = value; }

    [JsonIgnore]
    public EventsManager EventsManager
    {
      get => eventsManager;
      set
      {
        eventsManager = value;
        eventsManager.EventAppended += EventsManager_ActionAppended;
      }
    }

    public void Disconnect()
    {
      if (eventsManager != null)
        eventsManager.EventAppended -= EventsManager_ActionAppended;
    }

    private void EventsManager_ActionAppended(object sender, Roguelike.Events.GameEvent e)
    {
      if (e is InventoryAction)
      {
        var ia = e as InventoryAction;
        if (ia.Kind == InventoryActionKind.ItemRemoved && ia.Inv.Owner is Roguelike.Tiles.LivingEntities.Hero)
        {
          //reset IHotbarItem in bar
          //var item = CurrentItems.Cont.FirstOrDefault(i => i == ia.IHotbarItem);
          var item = ia.Loot;
          if (item != null)
          {
            if (!item.StackedInInventory || ia.Inv.GetStackedCount(item as Roguelike.Tiles.Looting.StackedLoot) == 0)
            {
              if (RemovedFromToolbarOnZeroCount(ia.Loot))
              {
                var digit = GetItemDigit(ia.Loot);
                if (IsDigitSupported(digit))
                  SetAt(digit, null);
              }
            }
          }
        }
      }
    }

    public static bool RemovedFromToolbarOnZeroCount(IHotbarItem loot)
    {
      if (loot is FightItem)//Abilities are assigned to bar withh 0 count
        return false;

      return true;
    }

    public static bool ShowImageAtZeroCount(IHotbarItem loot)
    {
      if (loot is FightItem)//Abilities are assigned to bar with 0 count
      {
        return true;
      }

      return false;
    }

    public static bool ShowZeroCount(IHotbarItem loot)
    {
      if (loot is FightItem)//Abilities are assigned to bar with 0 count
      {
        if (loot is ProjectileFightItem pfi && pfi.EndlessAmmo)
          return false;
        return true;
      }

      return false;
    }

    public int ItemCount(IHotbarItem item)
    {
      if (!HasItem(item))
      {
        return 0;
      }
      var stacked = item as StackedLoot;
      if (stacked != null)
        return stacked.Count;
      return 1;
    }

    public bool HasItem(IHotbarItem item)
    {
      return CurrentLine.HasItem(item);
    }

    public event EventHandler ActiveItemDigitSet;
    public int ActiveItemDigit
    {
      get => CurrentLine.ActiveItemDigit;
      set
      {
        CurrentLine.ActiveItemDigit = value;
        if (ActiveItemDigitSet != null)
          ActiveItemDigitSet(this, EventArgs.Empty);
      }
    }
    [JsonIgnore]
    public Func<IHotbarItem, bool> AutoSelectItem { get => autoSelectItem; set => autoSelectItem = value; }
    public ShortcutsBarContainer ItemsContainer { get => itemsContainer; set => itemsContainer = value; }

    public void SetCurrentContainerIndex(int index)
    {
      ItemsContainer.CurrentIndex = index;
      EventsManager.AppendAction(new ShortcutsBarAction() { Kind = ShortcutsBarActionKind.ContainerIndexChanged });
    }

    public void SetAtFirstFreeSlot(IHotbarItem item)
    {
      var keys = GetSupportedKeys();
      foreach (var key in keys)
      {
        var digit = GetDigit(key);
        var itemAt = GetAt(digit);
        if (itemAt == null || ItemCount(itemAt) == 0)
        {
          SetAt(digit, item);
          break;
        }
      }
    }

    public int GetProjectileDigit(FightItemKind fightItemKind)
    {
      if (fightItemKind == FightItemKind.Unset)
        return -1;
      return CurrentLine.GetProjectileDigit(fightItemKind);
    }

    public int GetItemDigit(IHotbarItem item)
    {
      return CurrentLine.GetItemDigit(item);
    }

    public static bool IsItemSelectable(IHotbarItem hotbarItem)
    {
      if (hotbarItem is SpellSource || hotbarItem is FightItem || (hotbarItem is Ability ab && ab is ActiveAbility))
        return true;

      return false;
    }

    public static bool IsAssignable(IHotbarItem item)
    {
      if (item is Consumable)
        return true;

      if (IsItemSelectable(item))
        return true;

      return false;
    }

    FightItem GetFightItem(FightItemKind fik, AdvancedLivingEntity ale)
    {
      //if (fightItemToAbilityMapping == FightItemToAbilityMapping.Unset || FightItem.GetAbilityKind(fik) == AbilityKind.Unset)
      {
        var digit = GetProjectileDigit(fik);
        if (digit >= 0)
        {
          //is in inv ?
          var stackedInInv = ale.Inventory.GetStacked<ProjectileFightItem>().Where(i => i.FightItemKind == fik).FirstOrDefault();
          if (stackedInInv == null && fik == FightItemKind.ThrowingTorch)
          {
            stackedInInv = ale.GetActiveEquipment(CurrentEquipmentKind.Shield) as ProjectileFightItem;
          }

          if (stackedInInv != null || fightItemToAbilityMapping == FightItemToAbilityMapping.Unset)
          {
            if (stackedInInv != null)
            {
              //var clone = stackedInInv.Clone(stackedInInv.Count) as FightItem;
              //if (fik == FightItemKind.ThrowingTorch)
              //{
              //  clone.Count = Owner.GetFightItemTotalCount(fik);
              //  return clone;
              //}
            }
            return stackedInInv;
          }

          if (fightItemToAbilityMapping == FightItemToAbilityMapping.AbilityToFightItem)
          {
            projSearch[fik].Count = 0;
            return projSearch[fik];
          }
        }

      }
      //else
      //{
      //  var ak = FightItem.GetAbilityKind(fik);
      //  var ab = ale.GetActiveAbility(ak);
      //  var abToolbar = GetItemDigitByName(ab.Name);
      //}
      return null;
    }

    public bool AssignItem(IHotbarItem item, int digit)
    {
      if (!OuaDII.LootContainers.ShortcutsBar.IsAssignable(item))
        return false;

      var ok = SetAt(digit, item);
      return ok;
    }
  }
}
