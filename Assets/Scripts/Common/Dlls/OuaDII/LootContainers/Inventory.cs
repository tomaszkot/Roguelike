using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract.HotBar;
using Roguelike.Events;
using Roguelike.LootContainers;
using Roguelike.Settings;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.LootContainers
{
  public class Inventory : Roguelike.LootContainers.Inventory
  {
    ShortcutsBar shortcutsBar;

    public Inventory(Container container) : base(container)
    {
    }

    [JsonIgnoreAttribute]
    public ShortcutsBar ShortcutsBar { get => shortcutsBar; set => shortcutsBar = value; }

    public override bool Add(Loot item, AddItemArg arg = null)
    {
      var itemStacked = item as StackedLoot;
      var added = base.Add(item, arg);

      if (added && Options.Instance.View.PlaceLootToShortcutBar && IsItemAutoputableToShortCutBar(itemStacked))
      {
        if (ShortcutsBar != null)
        {
          IHotbarItem itemToAdd = item;
          //if (item is ProjectileFightItem pfi)
          //  itemToAdd = GetShortcutsBarItem(pfi);
          ShortcutsBar.AddItem(itemToAdd);
        }
      }

      return added;
    }
        
    private bool IsItemAutoputableToShortCutBar(StackedLoot itemStacked)
    {
      if (itemStacked == null)
        return false;

      if (itemStacked is FightItem)
      {
        return true;
      }

      if (itemStacked is Mushroom mash)
      {
        if (mash.MushroomKind == MushroomKind.BlueToadstool ||
            mash.MushroomKind == MushroomKind.RedToadstool)
        {
          return false;
        }
      }

      if (itemStacked is Consumable)
      {
        if (itemStacked is Plant pl)
          return pl.IsConsumable();

        return true;
      }

      if (itemStacked is Potion)
        return true;

      if (itemStacked is SpellSource)
        return true;

      return false;
    }

    
  }
}
