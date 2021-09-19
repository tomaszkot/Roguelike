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
        Engaged.Add(new HistoryItem() { Tag1 = tag1 });
      }
    }
  }
}
