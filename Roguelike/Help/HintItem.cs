using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Help
{
  public enum HintKind
  {
    Unset, LootCollectShorcut, BulkLootCollectShorcut, ShowCraftingPanel, HeroLevelTooLow, CanNotPutOnUnidentified,
    LootHightlightShorcut
  }

  public class HintItem
  {
    public string Info { get; set; }
    public string Asset { get; set; }
    public bool Shown { get; set; }
    public HintKind Kind { get; set; }
    public int KeyCode { get; set; }
  }
}
