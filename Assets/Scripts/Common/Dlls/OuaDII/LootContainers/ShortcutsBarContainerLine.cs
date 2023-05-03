using Roguelike.Abstract.HotBar;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.LootContainers
{
  public class ShortcutsBarContainerLine
  {
    Dictionary<int, IHotbarItem> items = new Dictionary<int, IHotbarItem>();

    public ShortcutsBarContainerLine()
    {
      ActiveItemDigit = -1;
    }

    public int ActiveItemDigit
    {
      get;
      set;
    }

    public void SetAt(int digit, IHotbarItem IHotbarItem)
    {
      items[digit] = IHotbarItem;
    }

    public IHotbarItem GetAt(int digit)
    {
      var item = items.ContainsKey(digit) ? items[digit] : null;

      return item;
    }

    public int Count { get { return items.Count; } }

    public Dictionary<int, IHotbarItem> Items { get => items; set => items = value; }

    //public IEnumerable<KeyValuePair<int, IHotbarItem>> GetItemEnumByName(string name)
    //{
    //  return items.Where(i => i.Value != null && i.Value.Name == name);
    //}
        
    public T GetStacked<T>() where T : StackedLoot
    {
      return GetStackedItems<T>().FirstOrDefault();
    }
    public List<T> GetStackedItems<T>() where T : StackedLoot
    {
      return Items.Where(i => i.Value != null && i.Value.GetType() == typeof(T)).Select(i=>i.Value).Cast<T>().ToList();
    }

    public ProjectileFightItem GetProjectileFightItem(FightItemKind fik)
    {
      return GetStackedItems<ProjectileFightItem>().Where(i=>i.FightItemKind == fik).FirstOrDefault();
    }

    IEnumerable<KeyValuePair<int, IHotbarItem>> GetItemEnum(FightItemKind fik)
    {
      return Items.Where(i => i.Value != null && i.Value is ProjectileFightItem pfi && pfi.FightItemKind == fik);
    }

    public int GetProjectileDigit(FightItemKind fightItemKind)
    {
      var en = GetItemEnum(fightItemKind);
      if (en.Any())
        return en.First().Key;
      return -1;
    }

    public IEnumerable<KeyValuePair<int, IHotbarItem>> GetItemEnum(IHotbarItem item)
    {
      if (item is FightItem fi)
        return GetItemEnum(fi.FightItemKind);
      return items.Where(i => i.Value != null && i.Value.Equals(item));
    }

    public bool HasItem(IHotbarItem item)
    {
      return GetItemEnum(item).Any();
    }

    public int GetItemDigit(IHotbarItem item)
    {
      IEnumerable<KeyValuePair<int, IHotbarItem>> en;
      if (item is FightItem fi)
        en = GetItemEnum(fi.FightItemKind);
      else
        en = GetItemEnum(item);
      if (en.Any())
        return en.First().Key;
      return -1;
    }

    //public bool HasItem(string name)
    //{
    //  return GetItemEnumByName(name).Any();
    //}
  }
}
