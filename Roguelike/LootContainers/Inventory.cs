using Dungeons.ASCIIDisplay;
using Dungeons.ASCIIDisplay.Presenters;
using Newtonsoft.Json;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Roguelike.LootContainers
{
  public class RemoveItemArg
  {
    public bool DragDrop = false;
    public int StackedCount = 1;//in case of stacked there can be one than more sold at time

    public RemoveItemArg()
    {

    }
  }

  //public class CurrentEquipmentRemoveItemArg : AddItemArg
  //{
  //}

  public class AddItemArg
  {
    public bool notifyObservers = true;
    public bool justSwappingHeroInv = false;
    public InventoryActionDetailedKind detailedKind = InventoryActionDetailedKind.Unset;
  }

  public class CurrentEquipmentAddItemArg : AddItemArg
  {
    public CurrentEquipmentKind cek = CurrentEquipmentKind.Unset;
    public bool primary = true;
  }

  public enum InvOwner { Unset, Hero, Merchant }
  public enum InvBasketKind { Unset, Hero, Merchant, CraftingRecipe, CraftingInvItems, HeroChest, HeroEquipment, Ally, AllyEquipment }

  public interface IInventory
  {
    bool Remove(Loot loot, RemoveItemArg arg = null);
    float PriceFactor { get; set; }
  }

  public class Inventory : IInventory
  {
    public int CurrentPageIndex { get; set; }
    public float PriceFactor { get; set; } = 1;
    List<Loot> items = new List<Loot>();
    public int Capacity { get; set; }//how many items there can be?

    [JsonIgnore]
    public AdvancedLivingEntity Owner { get; set; }
    //public InvOwner InvOwner { get; set; }
    public InvBasketKind InvBasketKind { get; set; }

    [JsonIgnore]
    public EventsManager EventsManager
    {
      get { return Container.GetInstance<EventsManager>(); }
    }

    public Inventory(Container container)
    {
      this.Container = container;
      //Assert(this.Container != null);
      Capacity = 48;
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

    public StackedLoot GetStackedLoot(string lootName)
    {
      return Items.Where(i => i is StackedLoot).Where(i => i.Name == lootName).FirstOrDefault() as StackedLoot;
    }

    public int GetStackedCount(string lootName)
    {
      var stacked = Items.Where(i => i is StackedLoot sl).Where(i => i.Name == lootName).FirstOrDefault();
      return stacked != null ? GetStackedCount(stacked as StackedLoot) : 0;
    }

    public List<T> GetStacked<T>() where T : StackedLoot
    {
      return Items.Where(i => i.GetType() == typeof(T)).Cast<T>().ToList();
    }

    protected StackedLoot GetStackedItem(Loot loot)
    {
      return Items.FirstOrDefault(i => i == loot) as StackedLoot;
    }

    public virtual bool Add
    (
      Loot item,
      AddItemArg args = null
    )
    {
      if (args == null)
        args = new AddItemArg();
      //container.GetInstance<ILogger>().LogInfo("Add(Loot item)");
      var exist = false;
      var stackedInInv = GetStackedItem(item);
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
        if (Capacity <= ItemsCount)
        {
          Assert(false, "Capacity <= ItemsCount");
          return false;
        }

        if (item.StackedInInventory)
        {
          SetStackCount(itemStacked, itemStacked.Count);
        }
        else
        {
          Items.Add(item);
        }

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
          Assert(false, "Add(Loot item) duplicate item " + item);
          //throw new Exception("Add(Loot item) duplicate item " + item);
        }
      }

      if (changed && args.notifyObservers)
        AppendAction(new InventoryAction(this) { Kind = InventoryActionKind.ItemAdded, Loot = item, Inv = this, DetailedKind = args.detailedKind });

      return changed;
    }

    private void AppendAction(GameEvent ac)
    {
      if (EventsManager != null)
        EventsManager.AppendAction(ac);
      else
        Debug.Assert(false, Owner + " AppendAction EventsManager == null");
    }

    public void Assert(bool assert, string info = "assert failed")
    {
      if (!assert)
      {
        AppendAction(new Events.GameStateAction() { Type = Events.GameStateAction.ActionType.Assert, Info = info });
      }
    }

    public virtual bool Remove(Loot item, RemoveItemArg arg = null)//int stackedCount = 1
    {
      var res = false;
      if (arg == null)
        arg = new RemoveItemArg();
      //Assert(false, "loot");
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
          if (stackedItemCount >= arg.StackedCount)
          {
            Assert(stackedItemCount > 0);
            stackedItemCount -= arg.StackedCount;
            SetStackCount(stackedItem, stackedItemCount);
            if (stackedItemCount <= 0)
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
      if (sendSignal)
      {
        AppendAction(new InventoryAction(this) { Kind = InventoryActionKind.ItemRemoved, Loot = item, Inv = this });
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

    public Loot Get(Loot loot)
    {
      return Items.Where(i => i == loot).SingleOrDefault();
    }

    [JsonIgnore]
    public Container Container
    {
      get;
      set;
    }

    internal bool CanAddLoot(Loot loot)
    {
      return Capacity > Items.Count || (loot.StackedInInventory && GetStackedCount(loot as StackedLoot) > 0);//TODO stacked
    }

    public override string ToString()
    {
      if (Owner == null)
        return base.ToString();
      return Owner.ToString() + ", Count: " + Items.Count;
    }

    public bool CanAcceptItem(Loot loot, AddItemArg addItemArg)
    {
      return Owner.InventoryAcceptsItem(this, loot, addItemArg);
    }
  }
}
