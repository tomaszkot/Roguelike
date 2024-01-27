using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Dungeons.Core;
using Roguelike.Crafting.Workers;
using System.Diagnostics;
using Roguelike.Core.Crafting.Workers;

namespace Roguelike.Crafting
{
  public class RecipeRequiredItem
  {
    public Type Type { get; set; }
    public int MinCount { get; set; } = 1;

    public override string ToString()
    {
      return base.ToString() + "Type: "+ Type;
    }

  }

  public class RecipeRequiredItems
  {
    List<RecipeRequiredItem> items = new List<RecipeRequiredItem>();

    public List<RecipeRequiredItem> Items { get => items; set => items = value; }
  }

  public abstract class CraftWorker : LootCrafterTools
  {
    static List<RecipeKind> recipeKinds;

    protected List<Loot> lootToConvert;
    protected List<Equipment> eqs;
    protected List<Enchanter> enchanters;
    protected List<StackedLoot> stacked;
    protected ILootCrafter lootCrafter;

    public List<Loot> usedSrcItems = new List<Loot>();
    
    public RecipeKind Kind { get; set; }
    public List<Loot> LootToConvert 
    {
      get => lootToConvert; 
    }
    public List<Equipment> Eqs { get => eqs;  }

    static CraftWorker()
    {
      recipeKinds = EnumHelper.GetEnumValues<RecipeKind>(true);
      recipeKinds.Remove(RecipeKind.Custom);
    }

    public CraftingResult ReturnCanDo(CraftingResult previewResult)
    {
      return previewResult == null ? new CraftingResult(this.lootToConvert): previewResult;
    }

    public CraftWorker()
    { 
    }

    public static bool IsAdvisorSupportsBulk(RecipeKind rk)
    {
      if (rk == RecipeKind.EnchantEquipment)
        return false;
      if (rk == RecipeKind.TransformGem || rk == RecipeKind.TwoEq)
        return false;
      return true;
    }

    public CraftWorker(List<Loot> lootToConvert, ILootCrafter lootCrafter)
    {
      Init(lootToConvert, lootCrafter);
    }

    public virtual void Init(List<Loot> lootToConvert, ILootCrafter lootCrafter)
    {
      this.lootCrafter = lootCrafter;
      this.lootToConvert = lootToConvert;
      eqs = Filter<Equipment>(lootToConvert);
      enchanters = Filter<Enchanter>(lootToConvert);
      stacked = Filter<StackedLoot>(lootToConvert);
    }
    public override void ReportError(string errorMessage)
    {
      Debug.WriteLine("!!!" + errorMessage + ", "+Kind);
    }

    public abstract CraftingResult CanDo();

    public abstract CraftingResult Do();

    public abstract RecipeRequiredItems GetRequiredItems();
  }
}
