﻿using System.Collections.Generic;
using System.Linq;

namespace Roguelike
{
  namespace Hints
  {
    public enum HintKind { Unset, LootCollectShorcut, BulkLootCollectShorcut, ShowCraftingPanel }

    public class HintItem
    {
      public string Info { get; set; }
      public string Asset { get; set; }
      public bool Shown { get; set; }
      public HintKind Kind { get; set; }
      public int KeyCode { get; set; }
    }

    public class HintHistory
    {
      List<HintItem> hints = new List<HintItem>();

      public HintHistory()
      {
        //TODO 'G' - shall be formatted based on KeyCode
        hints.Add(new HintItem(){ Info = "Press 'G' to collect a single loot.", Kind = HintKind.LootCollectShorcut});
        hints.Add(new HintItem() { Info = "Press 'J' to collect nearby loot items.", Kind = HintKind.BulkLootCollectShorcut });
        hints.Add(new HintItem() { Info = "Recipe has been collected. Press 'R' to open Crafting Panel and see it's description.", Kind = HintKind.ShowCraftingPanel});
      }

      public List<int> GetKeyCodes()
      {
        return hints.Select(i => i.KeyCode).ToList();
      }

      public void SetKeyCode(HintKind kind, int keyCode)
      {
        var hint = Get(kind);
        if (hint == null)
          return;
        hint.KeyCode = keyCode;
      }

      public HintItem Get(int keyCode)
      {
        var hint = hints.Where(i => i.KeyCode == keyCode).FirstOrDefault();
        return hint;
      }

      public HintItem Get(HintKind kind)
      {
        var hint = hints.Where(i => i.Kind == kind).FirstOrDefault();
        return hint;
      }

      public void SetShown(string info)
      {
        var hint = hints.Where(i => i.Info == info).FirstOrDefault();
        if (hint == null)
          return;

        hint.Shown = true;
      }

      public void SetShown(HintKind kind)
      {
        var hint = Get(kind);
        if (hint == null)
          return;

        hint.Shown = true;
      }

      public bool WasShown(string info)
      {
        var hint = hints.Where(i => i.Info == info).FirstOrDefault();
        if (hint == null)
          return false;

        return hint.Shown;
      }

      public bool WasShown(HintKind kind)
      {
        var hint = Get(kind);
        if (hint == null)
          return false;

        return hint.Shown;

      }
    }
  }
}
