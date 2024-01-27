using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Core.Crafting.Workers
{
  internal class RechargeMagicalWeapon : CraftWorker
  {
    internal RechargeMagicalWeapon(List<Loot> lootToConvert, ILootCrafter lootCrafter) : base(lootToConvert, lootCrafter)
    {
      Kind = RecipeKind.RechargeMagicalWeapon;
    }

    public override RecipeRequiredItems GetRequiredItems()
    {
      var ri = new RecipeRequiredItems();
      var i1 = new RecipeRequiredItem() { Type = typeof(Weapon), MinCount = 1 };
      //var i3 = new RecipeRequiredItem() { Type = typeof(MagicDust), MinCount = 1 };
      ri.Items.Add(i1);
      //ri.Items.Add(i3);
      return ri;
    }

    public override CraftingResult CanDo()
    {
      CraftingResult previewResult = null;
      var equips = lootToConvert.Where(i => i is Weapon wpn0 && wpn0.IsMagician).Cast<Weapon>().ToList();
      var equipsCount = equips.Count();
      if (equipsCount != 1)
        return ReturnCraftingError("One charge emitting weapon is needed by the Recipe");
      return ReturnCanDo(previewResult);
    }

    public override CraftingResult Do()
    {
      var equips = lootToConvert.Where(i => i is Weapon wpn0 && wpn0.IsMagician).Cast<Weapon>().ToList();
      var wpn = equips[0];
      //foreach (var wpn in equips)
      {
        (wpn.SpellSource as WeaponSpellSource).RestoreCharges();
        wpn.UpdateMagicWeaponDesc();
      }
      return ReturnCraftedLoot(wpn, null, false);
    }
  }
}
