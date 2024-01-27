using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Crafting.Workers;
using Roguelike.Generators;
using Roguelike.LootFactories;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.Crafting
{
  public interface ILootCrafter
  {
    CraftingResult Craft(Recipe recipe, List<Loot> lootToCraft);
    bool ApplyEnchant(Enchanter enchant, Equipment eq, out string err);

    LootGenerator GetLootGenerator();
    Type GetMinedLootType();
  }
    
  public abstract class LootCrafterBase : LootCrafterTools, ILootCrafter 
  {
    public abstract bool ApplyEnchant(Enchanter enchant, Equipment eq, out string err);
    

    public abstract CraftingResult Craft(Recipe recipe, List<Loot> lootToCraft);

    public abstract LootGenerator GetLootGenerator();

    public virtual Type GetMinedLootType()
    {
      throw new NotImplementedException();
    }
  }

  public class LootCrafter : LootCrafterBase
  {
    Container container;
    EquipmentFactory equipmentFactory;

    public override LootGenerator GetLootGenerator()
    {
      return container.GetInstance<LootGenerator>();
    }
    public LootCrafter(Container container)
    {
      this.container = container;
      equipmentFactory = container.GetInstance<EquipmentFactory>();
    }

    protected virtual EquipmentKind GetEquipmentKindForGemApply(Equipment eq)
    {
      return eq.EquipmentKind;
    }

    public override bool ApplyEnchant(Enchanter enchant, Equipment eq, out string err)
    {
      return enchant.ApplyTo(eq, () => { return GetEquipmentKindForGemApply(eq); }, out err);
    }
    
    protected List<Loot> orgLootToConvert;
    protected MagicDust magicDust;
    protected Recipe orgRecipe;
    public override CraftingResult Craft(Recipe recipe, List<Loot> lootToConvert)
    {
      if (recipe == null)
        return ReturnCraftingError("Recipe not set");

      orgRecipe = recipe;
      magicDust = FilterOne<MagicDust>(lootToConvert);
      orgLootToConvert = lootToConvert.ToList();
      lootToConvert = lootToConvert.Where(i => !(i is MagicDust)).ToList();
      var eqs = Filter<Equipment>(lootToConvert);
      
      if (eqs.Any(i => i.Class == EquipmentClass.Unique))
      {
        string error;
        var unqs = eqs.Where(i => i.Class == EquipmentClass.Unique).ToList();
        if (unqs.Count > 1 || !unqs[0].CanBeEnchantedDueToClass(out error))
          return ReturnCraftingError("Unique items can not crafted");
      }
      if (lootToConvert.Any())
      {
        return Convert(recipe, lootToConvert);
      }

      return ReturnCraftingError(InvalidIngredients);
    }

    protected virtual CraftingResult Convert(Recipe recipe, List<Loot> lootToConvert)
    {
      var fac = new WorkersFactory(lootToConvert, this, this.container);
      var previewRes = ReturnCraftingError("Unsupported operation");
      var worker = fac.GetWorker(recipe.Kind);

      if (worker != null)
      {
        if (worker.Kind != recipe.Kind)
          recipe = new Recipe(worker.Kind);

        if (magicDust == null || magicDust.Count < orgRecipe.MagicDustRequired)
        {
          return ReturnCraftingError("Invalid amount of Magic Dust");// + worker.Kind); - do not show it to user
        }

        previewRes = worker.CanDo();
        if (previewRes.Success)
        {
          Debug.WriteLine("Do " + worker.Kind);
          var did = worker.Do();
          if (did.Success)
          {
            did.UsedKind = worker.Kind;
            if (did.UsedInputItems == null)
            {
              var reqItems = worker.GetRequiredItems();
              did.UsedInputItems = orgLootToConvert.Where(i => reqItems.Items.Any(i => i.Type == i.GetType())).ToList();
            }
          }
          return did;
        }
      }
      return previewRes;
    }

    protected virtual CraftingResult ConvertMold(Recipe recipe, List<Loot> lootToConvert)
    {
      return ReturnCraftingError("TODO");
    }

    protected virtual CraftingResult ConvertBoltsOrArrows(Recipe recipe, List<Loot> lootToConvert)
    {
      return ReturnCraftingError("TODO");
    }
          

     private CraftingResult ReturnStealingEq(List<Equipment> eqs, List<SpecialPotion> sps)
    {
      if (eqs[0].ExtendedInfo.Stats.GetFactor(EntityStatKind.LifeStealing) > 10 ||
                    eqs[0].ExtendedInfo.Stats.GetFactor(EntityStatKind.ManaStealing) > 10)
      {
        return ReturnCraftingError("Max level of one of statistics reached");
      }

      float ls = 0;
      float ms = 0;
      foreach (var sp in sps)
      {
        var esk = sp.SpecialPotionKind == Tiles.Looting.SpecialPotionKind.Strength ? EntityStatKind.LifeStealing : EntityStatKind.ManaStealing;
        if (esk == EntityStatKind.LifeStealing)
          ls += sp.GetEnhValue();
        else
          ms += sp.GetEnhValue();

      }
      if (ls > 0)
      {
        eqs[0].AddMagicStat(EntityStatKind.LifeStealing);
        eqs[0].ExtendedInfo.Stats[EntityStatKind.LifeStealing].Factor = ls;
      }
      if (ms > 0)
      {
        eqs[0].AddMagicStat(EntityStatKind.ManaStealing);
        eqs[0].ExtendedInfo.Stats[EntityStatKind.ManaStealing].Factor = ms;
      }

      return ReturnCraftedLoot(eqs[0]);
    }

  }
}
