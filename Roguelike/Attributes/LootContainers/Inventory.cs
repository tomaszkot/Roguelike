using Dungeons.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Tiles;
using Dungeons.ASCIIDisplay.Presenters;
using Dungeons.ASCIIDisplay;

namespace Roguelike.LootContainers
{
  public class Inventory
  {
    public int CurrentPageIndex { get; set; }
    public float PriceFactor { get; set; }
    public event EventHandler<GenericEventArgs<Tuple<Loot, bool>>> ItemsChanged;
    List<Loot> items = new List<Loot>();

    public Inventory()
    {
      PriceFactor = 1;
    }

    public List<ListItem> ToASCIIList()
    {
      List<ListItem> list = this.Items.Select(i => new ListItem() { Text = i.ToString() }).ToList();
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

    //[XmlIgnore]
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
        return Items.Where(i => i.Equals(loot)).Count();
      }

      return 0;
    }
    public virtual int GetTypedStackCount<T>()
    {
      return Items.Where(i => i is T).Count();
    }

    public virtual bool Add(Loot item, bool notifyObservers = true, bool justSwappingHeroInv = false)
    {
      //Debug.WriteLine("Add(Loot item) " + Thread.CurrentThread.ManagedThreadId);
      if (!Items.Contains(item) || item.StackedInInventory)
      {
        item.Collected = true;
        Items.Add(item);
        if (notifyObservers && ItemsChanged != null)
        {
          var tuple = new Tuple<Loot, bool>(item, true);
          ItemsChanged(this, new GenericEventArgs<Tuple<Loot, bool>>(tuple));
        }

        return true;
      }
      else
      {
        var sameID = Items.FirstOrDefault(i => i.Id == item.Id);
        ////Debug.WriteLine("id = "+ item.Id + "sameID = "+ sameID);
        //Assert(false, "Add(Loot item) duplicate item " + item);
        //throw new Exception("Add(Loot item) duplicate item " + item);
      }
      return false;
    }

    

    //public List<Loot> UnreportedRemovals = new List<Loot>();

    IEnumerable<Loot> GetStackedItems(Loot item)
    {
      return Items.Where(i => i.Equals(item));
    }

    public Loot GetFirstStackedItem(Loot item)
    {
      return GetStackedItems(item).FirstOrDefault();
    }

    private int GetStackedInfoByLoot(Loot item)
    {
      var count = GetStackedItems(item).Count();
      return count;
    }

    public bool Remove(Loot item)
    {
      var res = false;
      if (item.StackedInInventory)
      {
        var stackedInfo = Items.FirstOrDefault(i => i == item);
        if (stackedInfo != null)
        {
          res = Items.Remove(stackedInfo);
          if (!res)
          {
            ;//Assert(false);
          }
        }
      }
      else
      {
        res = Items.Remove(item);
        if (!res)
        {
          //Assert(false);
          return false;
        }
      }
      if (ItemsChanged != null)
      {
        var tuple = new Tuple<Loot, bool>(item, false);
        ItemsChanged(this, new GenericEventArgs<Tuple<Loot, bool>>(tuple));
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
        var plainItemsCount = Items.Where(i => !i.StackedInInventory).Count();
        var stackedCount = Items.Where(i => i.StackedInInventory).GroupBy(j => j.StackedInventoryId).Count();
        return plainItemsCount + stackedCount;
      }

    }
  }
}
