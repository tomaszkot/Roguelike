﻿using Roguelike.Help;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike
{
  namespace History
  {
    namespace Hints
    {
      public class HintHistory
      {
        //public static Dictionary<HintKind, string> Messages = new Dictionary<HintKind, string>();
        private List<HintItem> hints = new List<HintItem>();

        //[Json]
        public List<HintItem> Hints
        {
          get => hints;
          set
          {
            hints = value;
          }
        }

        static HintHistory()
        {
          //Messages.Add(HintKind.HeroLevelTooLow, "Hero level too low to use an item");
          //Messages.Add(HintKind.CanNotPutOnUnidentified, "Can not put on unidentified item");
        }

        public HintHistory()
        {
          Hints.Add(new HintItem() { Info = "Press 'Left Alt' to see collectable/interactive items.", Kind = HintKind.LootHightlightShortcut });
          Hints.Add(new HintItem() { Info = "Press 'G' to collect a loot under your position.", Kind = HintKind.LootCollectShortcut });
          Hints.Add(new HintItem() { Info = "Use 'G' to collect nearby loot items.", Kind = HintKind.BulkLootCollectShortcut });
          Hints.Add(new HintItem() { Info = "Recipe has been collected. Press 'R' to open Crafting Panel and see it's description.", Kind = HintKind.ShowCraftingPanel });
          Hints.Add(new HintItem() { Info = "Hero level too low to use an item", Kind = HintKind.HeroLevelTooLow });
          Hints.Add(new HintItem() { Info = "Can not put on unidentified item", Kind = HintKind.CanNotPutOnUnidentified });
          Hints.Add(new HintItem() { Info = "TODO", Kind = HintKind.UseProjectile });
          Hints.Add(new HintItem() { Info = "TODO", Kind = HintKind.UseElementalWeaponProjectile });
          Hints.Add(new HintItem() { Info = "swapping an active weapon/shield set Press 'X' to .", Kind = HintKind.SwapActiveWeapon });
          Hints.Add(new HintItem() { Info = "", Kind = HintKind.SecretLevel});
          Hints.Add(new HintItem() { Info = "You collected a gem, it can be used to enchant your equipment.\r\nPress 'I' to open Inventory Panel and drop the gem on a weapon.", Kind = HintKind.EnchantEquipment });
        }

        string BuildDesc(HintKind kind, int keyCode, Func<HintKind, string> codeFormatter = null)
        {
          string desc = "";
          switch (kind)
          {
            case HintKind.Unset:
              break;
            case HintKind.LootCollectShortcut:
              desc = "Press '{0}' to collect a single loot.";
              break;
            case HintKind.BulkLootCollectShortcut:
              desc = "Press '{0}' to collect nearby loot items.";
              break;
            case HintKind.ShowCraftingPanel:
              desc = "Recipe has been collected. Press '{0}' to open Crafting Panel and see it's description.";
              break;
            case HintKind.LootHightlightShortcut:
              desc = "Press '{0}' to see collectable/interactive items.";
              break;
            case HintKind.SwapActiveWeapon:
              desc = "You can press '{0}' to swap an active weapon/shield set.";
              break;
            default:
              break;
          }
          if (codeFormatter != null)
          {
            string code = codeFormatter(kind);
            desc = string.Format(desc, code);
          }
          else
            desc = string.Format(desc, (char)keyCode);
          
          return desc;
        }

        public List<int> GetKeyCodes()
        {
          return Hints.Select(i => i.KeyCode).ToList();
        }

        public void SetKeyCode(HintKind kind, int keyCode, Func<HintKind, string> codeFormatter)
        {
          var hint = Get(kind);
          if (hint == null)
            return;
          hint.KeyCode = keyCode;
          codeFormatters[kind] = codeFormatter;
          hint.Info = BuildDesc(kind, keyCode, codeFormatters[kind]);
        }

        Dictionary<HintKind, Func<HintKind, string>> codeFormatters = new Dictionary<HintKind, Func<HintKind, string>>();
        public void SetKeyCodeProvider(HintKind kind, Func<HintKind, string> codeFormatter)
        {
          codeFormatters[kind] = codeFormatter;
        }

        public HintItem Get(int keyCode)
        {
          var hint = Hints.Where(i => i.KeyCode == keyCode).FirstOrDefault();
          return hint;
        }

        public HintItem Get(HintKind kind)
        {
          var hint = Hints.Where(i => i.Kind == kind).FirstOrDefault();
          return hint;
        }

        public void SetShown(string info)
        {
          var hint = Hints.Where(i => i.Info == info).FirstOrDefault();
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
          var hint = Hints.Where(i => i.Info == info).FirstOrDefault();
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
}