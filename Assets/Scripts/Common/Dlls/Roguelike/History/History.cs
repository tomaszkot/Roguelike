using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike
{
  namespace History
  {
    public class HistoryContent
    {
      public Hints.HintHistory Hints { get; set; } = new Hints.HintHistory();
      public LootHistory Looting { get; set; } = new LootHistory();
      public LivingEntityHistory LivingEntity { get; set; } = new LivingEntityHistory();
      public List<HistoryItem> Engaged = new List<HistoryItem>();

      public bool WasEngaged(string tag1)
      {
        return Engaged.Any(i=>i.Tag1 == tag1);
      }

      public void SetEngaged(string tag1)
      {
        if(GetEngaged(tag1) is null)
          Engaged.Add(new HistoryItem() { Tag1 = tag1 });
      }

      public HistoryItem GetEngaged(string v)
      {
        var item = Engaged.Where(i => i.Tag1 == v).FirstOrDefault();
        return item;
      }

      public void RemoveEngaged(string v)
      {
        var item = GetEngaged(v);
        if (item!=null)
          Engaged.Remove(item);
      }

      public void EnsureEngaged(string v)
      {
        if (!WasEngaged(v))
          SetEngaged(v);
      }
    }
  }
}
