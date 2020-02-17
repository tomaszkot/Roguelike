using Dungeons.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Tiles;
using Dungeons.ASCIIDisplay.Presenters;
using Dungeons.ASCIIDisplay;
using Roguelike.Managers;
using Roguelike.Events;

namespace Roguelike.LootContainers
{
  public class Inventory
  {
    public int CurrentPageIndex { get; set; }
    public float PriceFactor { get; set; }
   // public event EventHandler<Tuple<Loot, bool>> ItemsChanged;
    List<Loot> items = new List<Loot>();
    Dictionary<Type, int> stackedCount = new Dictionary<Type, int>();
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

    public virtual int GetStackCount(Loot loot)
    {
      if (loot.StackedInInventory)
      {
        return GetStackedCountForType(loot.GetType());
      }

      return 0;
    }
    public virtual int GetTypedStackCount<T>()
    {
      var type = typeof(T);
      return GetStackedCountForType(type);
    }

    private int GetStackedCountForType(Type type)
    {
      if (stackedCount.ContainsKey(type))
        return stackedCount[type];
      return 0;
    }

    public virtual bool Add(Loot item, bool notifyObservers = true, bool justSwappingHeroInv = false)
    {
      //Debug.WriteLine("Add(Loot item) " + Thread.CurrentThread.ManagedThreadId);
      //|| item.StackedInInventory
      if (!Items.Contains(item))
      {
        item.Collected = true;
        Items.Add(item);
        if (item.StackedInInventory)
          stackedCount[item.GetType()] = 1;
        if (notifyObservers && EventsManager != null)
        {
          EventsManager.AppendAction(new InventoryAction(this) { Kind = InventoryActionKind.ItemAdded, Item = item});
          //var tuple = new Tuple<Loot, bool>(item, true);
          //ItemsChanged(this, tuple);
        }

        return true;
      }
      else
      {
        if (item.StackedInInventory)
        {
          var stackedItemCount = stackedCount[item.GetType()];
          Assert(stackedItemCount > 0);
          stackedCount[item.GetType()] += 1;
        }
        else
        {
          //var sameID = Items.FirstOrDefault(i => i.Id == item.Id);
          ////Debug.WriteLine("id = "+ item.Id + "sameID = "+ sameID);
          Assert(false, "Add(Loot item) duplicate item " + item);
          //throw new Exception("Add(Loot item) duplicate item " + item);
        }

      }
      return false;
    }

    

    //public List<Loot> UnreportedRemovals = new List<Loot>();

    //IEnumerable<Loot> GetStackedItems(Loot item)
    //{
    //  return Items.Where(i => i.Equals(item));
    //}

    //public Loot GetStackedItem(Loot item)
    //{
    //  return GetStackedItems(item).FirstOrDefault();
    //}

    //private int GetStackedInfoByLoot(Loot item)
    //{
    //  var count = GetStackedItems(item).Count();
    //  return count;
    //}

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
        var stackedItem = Items.FirstOrDefault(i => i == item);
        if (stackedItem != null)
        {
          if (stackedCount.ContainsKey(stackedItem.GetType()))
          {
            var stackedItemCount = stackedCount[stackedItem.GetType()];
            Assert(stackedItemCount > 0);
            if (stackedItemCount > 0)
            {
              stackedItemCount--;
              stackedCount[stackedItem.GetType()] = stackedItemCount;
              if(stackedItemCount == 0)
                itemToRemove = item;

              sendSignal = true;
            }
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

    //public Loot GetAt(int index)
    //{
    //  return Items[index];
    //}

    public bool Contains(Loot item)
    {
      return Items.Contains(item);
    }

    //public bool Contains(string AssetName)
    //{
    //  return Items.Any(i => i.AssetName == AssetName);
    //}

    public Loot GetNextLoot(Loot prev)
    {
      if (prev == null)
      {
        return Items.FirstOrDefault();
      }
      var ind = Items.IndexOf(prev);
      return Items.Count > ind + 1 ? Items[ind + 1] : Items.FirstOrDefault();
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
        //var plainItemsCount = Items.Count();

        //var stackedCount = Items.Where(i => i.StackedInInventory).GroupBy(j => j.StackedInventoryId).Count();
        //return plainItemsCount + stackedCount;
      }

    }
  }
}
