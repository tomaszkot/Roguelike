using Roguelike.Help;
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
          //Hints.Add(new HintItem() { Info = "Press 'Left Alt' to see collectable/interactive items.", Kind = HintKind.LootHightlightShorcut });
          Hints.Add(new HintItem() { Info = "Press 'G' to collect a single loot.", Kind = HintKind.LootCollectShorcut });
          //Hints.Add(new HintItem() { Info = "Press 'J' to collect nearby loot items.", Kind = HintKind.BulkLootCollectShorcut });
          //Hints.Add(new HintItem() { Info = "Recipe has been collected. Press 'R' to open Crafting Panel and see it's description.", Kind = HintKind.ShowCraftingPanel });

          //Hints.Add(new HintItem() { Info = Messages[HintKind.HeroLevelTooLow], Kind = HintKind.HeroLevelTooLow });
          //Hints.Add(new HintItem() { Info = Messages[HintKind.CanNotPutOnUnidentified], Kind = HintKind.CanNotPutOnUnidentified });
          ////Hints.Add(new HintItem() { Info = "TODO", Kind = HintKind.UseProjectile });
          ////Hints.Add(new HintItem() { Info = "TODO", Kind = HintKind.UseElementalWeaponProjectile });

          //Hints.Add(new HintItem() { Info = "", Kind = HintKind.LootHightlightShorcut });
          //Hints.Add(new HintItem() { Info = "", Kind = HintKind.LootCollectShorcut });
          //Hints.Add(new HintItem() { Info = "", Kind = HintKind.BulkLootCollectShorcut });
          //Hints.Add(new HintItem() { Info = "", Kind = HintKind.ShowCraftingPanel });

          //Hints.Add(new HintItem() { Info = Messages[HintKind.HeroLevelTooLow], Kind = HintKind.HeroLevelTooLow });
          //Hints.Add(new HintItem() { Info = Messages[HintKind.CanNotPutOnUnidentified], Kind = HintKind.CanNotPutOnUnidentified });
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
