using System.Collections.Generic;
using System.Linq;
using Roguelike.Tiles;
using Dungeons.ASCIIDisplay.Presenters;
using Dungeons.ASCIIDisplay;
using Roguelike.Managers;
using Roguelike.Events;
using Newtonsoft.Json;
using Roguelike.Tiles.Looting;
using System;

namespace Roguelike.LootContainers
{
  public class InventoryBase
  {
  }

  public class Inventory : InventoryBase
  {
    public int CurrentPageIndex { get; set; }
    public float PriceFactor { get; set; }
    List<Loot> items = new List<Loot>();
    public int Capacity { get; set; }//how many items there can be?

    [JsonIgnore]
    public EventsManager EventsManager { get; set; }

    public Inventory()
    {
      PriceFactor = 1;
    }

    public List<ListItem> ToASCIIList()
    {
      var list = this.Items.Select(i => new ListItem() { Text = i.ToString() }).ToList();
      return list;
    }

    public void SetPositionInPage(Loot item, int posInPage)
    {
      item.PositionInPage = posInPage;
      item.PageIndex = CurrentPageIndex;
    }

    public List<T> GetLootForCurrentPage<T>()
    {
      return GetLootForPage<T>(CurrentPageIndex);
    }

    public List<T> GetLootForPage<T>(int page)
    {
      var loots = Items.Where(i => i is T).ToList();
      return loots.Where(i => i.PageIndex == page).Cast<T>().ToList();
    }

    public int GetItemSellPrice(Loot loot)
    {
      return (int)(loot.Price * PriceFactor);
    }

    public List<Loot> Items
    {
      get
      {
        return items;//public member for serialization purposes!!!
      }

      set
      {
        items = value;
      }
    }

    //internal void UpdateScrollsLevel(LivingEntity le)
    //{
    //  var scrolls = Items.Where(i => i is Scroll).Cast<Scroll>().ToList();
    //  foreach (var scroll in scrolls)
    //  {
    //    scroll.UpdateLevel(le);
    //  }
    //}

    public IEnumerable<Loot> GetItems()
    {
      return items;
    }

    public IEnumerable<T> GetItems<T>()
    {
      return Items.Where(i => i is T).Cast<T>();
    }

    public virtual void SetStackCount(StackedLoot loot, int count)
    {
      var stackedItem = GetStackedItem(loot);
      if (stackedItem == null)
      {
        Items.Add(loot);
        stackedItem = loot;
      }
      stackedItem.Count = count;
    }

    public int GetStackedCount(StackedLoot loot)
    {
      var stackedItem = GetStackedItem(loot);
      if (stackedItem != null)
        return stackedItem.Count;

      return 0;
    }

    private StackedLoot GetStackedItem(Loot loot)
    {
      return Items.FirstOrDefault(i => i == loot) as StackedLoot;
    }

    public virtual bool Add(Loot item, bool notifyObservers = true, bool justSwappingHeroInv = false)
    {
      //Debug.WriteLine("Add(Loot item) " + Thread.CurrentThread.ManagedThreadId);
      var exist = false;
      StackedLoot stackedInInv = GetStackedItem(item);
      var itemStacked = item as StackedLoot;
      if (stackedInInv != null)
      {
        exist = true;
      }
      else
        exist = Items.Contains(item);

      bool changed = false;

      if (!exist)
      {
        //item.Collected = true;
        if (item.StackedInInventory)
        {
          SetStackCount(itemStacked, itemStacked.Count);
        }
        else
          Items.Add(item);

        changed = true;
      }
      else
      {
        if (stackedInInv != null)
        {
          //var stackedItemCount = GetStackCount(item);
          //Assert(stackedItemCount > 0);
          SetStackCount(stackedInInv, stackedInInv.Count + itemStacked.Count);
          changed = true;
        }
        else
        {
          //var sameID = Items.FirstOrDefault(i => i.Id == item.Id);
          ////Debug.WriteLine("id = "+ item.Id + "sameID = "+ sameID);
          Assert(false, "Add(Loot item) duplicate item " + item);
          //throw new Exception("Add(Loot item) duplicate item " + item);
        }
      }

      if (changed && notifyObservers && EventsManager != null)
        EventsManager.AppendAction(new InventoryAction(this) { Kind = InventoryActionKind.ItemAdded, Item = item });

      return changed;
    }

    public void Assert(bool assert, string info = "assert failed")
    {
      if (EventsManager != null && !assert)
      {
        EventsManager.AppendAction(new Events.GameStateAction() { Type= Events.GameStateAction.ActionType.Assert, Info = info });
      }
    }

    public bool Remove(Loot item)
    {
      var res = false;
      Assert(false, "loot");
      Loot itemToRemove = item;
      bool sendSignal = true;
      if (item.StackedInInventory)
      {
        itemToRemove = null;
        sendSignal = false;
        var stackedItem = GetStackedItem(item);
        if (stackedItem != null)
        {
          var stackedItemCount = GetStackedCount(stackedItem);
          if (stackedItemCount > 0)
          {
            Assert(stackedItemCount > 0);
            stackedItemCount--;
            SetStackCount(stackedItem, stackedItemCount);
            if(stackedItemCount == 0)
              itemToRemove = item;

            sendSignal = true;
            res = true;
          }
          else
            Assert(false);
        }
        else
          Assert(false);
      }

      if (itemToRemove != null)
      {
        res = Items.Remove(item);
        if (!res)
        {
          Assert(false);
          return false;
        }
      }
      if (sendSignal && EventsManager != null)
      {
        EventsManager.AppendAction(new InventoryAction(this) { Kind = InventoryActionKind.ItemRemoved, Item = item });
      }
      //else
      //  UnreportedRemovals.Add(item);
      return res;
    }

    public bool Contains(Loot item)
    {
      return Items.Contains(item);
    }

    public void Print(IDrawingEngine printer, string name)
    {
      printer.WriteLine(name + "[" + Items.Count + "]");
      Items.ForEach(i => printer.WriteLine(i.ToString()));
    }

    public int ItemsCount
    {
      get
      {
        return Items.Count;
      }
    }

    internal bool CanAddLoot(Loot loot)
    {
      return Capacity > Items.Count;//TODO stacked
    }
  }
}
