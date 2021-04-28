using System.Collections.Generic;

namespace Roguelike.UI.Models
{
  public class ListItemModel
  {
    public string DisplayValue { get; set; }
    public string ReturnValue { get; set; }
    public bool Disabled { get; internal set; }

    public ListItemModel(string returnValue, string displayValue = null)
    {
      DisplayValue = displayValue ?? returnValue;
      ReturnValue = returnValue;
    }

    public override string ToString()
    {
      return ReturnValue + " " + DisplayValue;
    }
  }

  public class ListModel
  {
    List<ListItemModel> items = new List<ListItemModel>();

    public virtual IEnumerable<ListItemModel> Items
    {
      get => items;
    }

    public void Add(ListItemModel item)
    {
      items.Add(item);
    }

    public virtual int Count()
    {
      return items.Count;
    }

    public virtual void Clear()
    {
      items.Clear();
    }

    public virtual void SetItems(List<ListItemModel> items)
    {
      this.items = items;
    }
  }
}
