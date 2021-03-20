using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.UI.Models
{
  public class GenericListItemModel<T> : ListItemModel
  {
    public T Item { get; private set; }

    public GenericListItemModel(T item, string returnValue, string displayValue = null)
      : base(returnValue, displayValue)
    {
      this.Item = item;
    }
  }

  public class GenericListModel<T> : ListModel
  {
    public List<GenericListItemModel<T>> TypedItems 
    { 
      get; 
      private set; 
    } = new List<GenericListItemModel<T>>();

    public GenericListModel()
    {
      TypedItems = new List<GenericListItemModel<T>>();
    }

    public GenericListModel(List<GenericListItemModel<T>> items)
    {
      TypedItems = items;
    }

    public override int Count()
    {
      return TypedItems.Count;
    }

    public void Add(GenericListItemModel<T> item)
    {
      TypedItems.Add(item);
      //base.Add(item);
    }

    public override void Clear()
    {
      TypedItems.Clear();
      base.Clear();
    }

    public override IEnumerable<ListItemModel> Items
    {
      get => TypedItems.Cast<ListItemModel>().ToList(); 
    }
  }
}
