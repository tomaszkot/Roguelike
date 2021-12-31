using Roguelike.Help;
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
          //TODO 'G' - shall be formatted based on KeyCode
          //Hints.Add(new HintItem() { Info = "Press 'Left Alt' to see collectable/interactive items.", Kind = HintKind.LootHightlightShortcut });
          Hints.Add(new HintItem() { Info = "Press 'G' to collect a single loot.", Kind = HintKind.LootCollectShortcut });
          //Hints.Add(new HintItem() { Info = "Press 'J' to collect nearby loot items.", Kind = HintKind.BulkLootCollectShortcut });
          //Hints.Add(new HintItem() { Info = "Recipe has been collected. Press 'R' to open Crafting Panel and see it's description.", Kind = HintKind.ShowCraftingPanel });

          //Hints.Add(new HintItem() { Info = Messages[HintKind.HeroLevelTooLow], Kind = HintKind.HeroLevelTooLow });
          //Hints.Add(new HintItem() { Info = Messages[HintKind.CanNotPutOnUnidentified], Kind = HintKind.CanNotPutOnUnidentified });
          ////Hints.Add(new HintItem() { Info = "TODO", Kind = HintKind.UseProjectile });
          ////Hints.Add(new HintItem() { Info = "TODO", Kind = HintKind.UseElementalWeaponProjectile });

          //Hints.Add(new HintItem() { Info = "", Kind = HintKind.LootHightlightShortcut });
          //Hints.Add(new HintItem() { Info = "", Kind = HintKind.LootCollectShortcut });
          //Hints.Add(new HintItem() { Info = "", Kind = HintKind.BulkLootCollectShortcut });
          //Hints.Add(new HintItem() { Info = "", Kind = HintKind.ShowCraftingPanel });

          //Hints.Add(new HintItem() { Info = Messages[HintKind.HeroLevelTooLow], Kind = HintKind.HeroLevelTooLow });
          //Hints.Add(new HintItem() { Info = Messages[HintKind.CanNotPutOnUnidentified], Kind = HintKind.CanNotPutOnUnidentified });
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
              break;
            case HintKind.ShowCraftingPanel:
              break;
            case HintKind.HeroLevelTooLow:
              break;
            case HintKind.CanNotPutOnUnidentified:
              break;
            case HintKind.LootHightlightShortcut:
              break;
            case HintKind.UseProjectile:
              break;
            case HintKind.UseElementalWeaponProjectile:
              break;
            case HintKind.SwapActiveWeapon:
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

        public void SetKeyCode(HintKind kind, int keyCode)
        {
          var hint = Get(kind);
          if (hint == null)
            return;
          hint.KeyCode = keyCode;
          if (!codeFormatters.ContainsKey(kind))
            codeFormatters[kind] = null;
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
