using Roguelike.Crafting;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roguelike.Extensions;
using System.Diagnostics;
//using System.Reflection.Metadata;

namespace Roguelike.Crafting
{
  public class LootCrafterTools
  {
    protected const string InvalidIngredients = "Invalid ingredients";
    

    protected int GetEnhStatValue(float currentVal, float betterEqPrice, float worseEqPrice)
    {
      float factor = 10;
      factor += factor * (worseEqPrice / betterEqPrice);
      if (factor > 16)
        factor = 16;
      var enh = currentVal * factor / 100f;
      if (enh < 1)
        enh = 1;

      return (int)Math.Ceiling(enh);
    }
    protected bool IsMatchingRecipe(List<Loot> lootToConvert, RecipeKind kind)
    {
      if (lootToConvert.Count == 2)
      {
        if (lootToConvert[0].GetMatchingRecipe(lootToConvert[1]) == kind)
          return true;
      }
      return false;
    }

    public List<T> Filter<T>(List<Loot> lootToConvert)
    {
      return lootToConvert.Where(i => i is T).Cast<T>().ToList();
    }
    protected T FilterOne<T>(List<Loot> lootToConvert)
    {
      return lootToConvert.Where(i => i is T).Cast<T>().FirstOrDefault();
    }

    protected int GetStackedCount<T>(List<Loot> lootToConvert, string name = "") where T : StackedLoot
    {
      if (name.Any())
      {
        lootToConvert = lootToConvert.Where(i => i.Name == name).ToList();
      }
      var stacked = Filter<T>(lootToConvert).FirstOrDefault();
      return stacked != null ? stacked.Count : 0;
    }

    protected List<Mushroom> GetToadstools(List<Loot> lootToConvert)
    {
      return lootToConvert.Where(i => i.IsToadstool()).Cast<Mushroom>().ToList();
    }

    protected int GetToadstoolsCount(List<Loot> lootToConvert)
    {
      var toadstools = lootToConvert.Where(i => i.IsToadstool()).ToList();
      return GetStackedCount<StackedLoot>(toadstools);
    }

    public virtual CraftingResult ReturnCraftingError(string errorMessage)
    {
      var error = new CraftingResult(new List<Loot>());
      error.Message = errorMessage;
      ReportError(errorMessage);
      return error;
    }

    public virtual void ReportError(string errorMessage)
    {
      Debug.WriteLine("!!!" + errorMessage);
    }

    public Loot GetMaxStackedCount(Loot[] lootToConvert)
    {
      var nn = Filter<StackedLoot>(lootToConvert.ToList());
      return nn.Where(i=> i.Count == nn.Max(i=>i.Count)).FirstOrDefault();
    }

    public int GetCraftedStackedCount(Loot[] lootToConvert)
    {
      return GetCraftedStackedCount(lootToConvert.ToList());
    }
    public int GetCraftedStackedCount(int[] lootToConvertCounts)
    {
      return lootToConvertCounts.Min();
    }

    public int GetCraftedStackedCount(List<Loot> lootToConvert)
    {
      return lootToConvert.Cast<StackedLoot>().Where(i=> i != null).Min(i => i.Count);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loot"></param>
    /// <param name="deleteCraftedLoot">Normally true but in rare cases when Eq is enhanced/fixed (e.g. Magical weapon recharge) false</param>
    /// <returns></returns>
    public CraftingResult ReturnCraftedLoot(Loot loot, List<Loot> usedSrcItems = null, bool deleteCraftedLoot = true)
    {
      if (loot == null)//ups
        return ReturnCraftingError("Improper ingredients");
     
      return ReturnCraftedLoot(new List<Loot>() { loot }, usedSrcItems, deleteCraftedLoot);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loot"></param>
    /// <param name="deleteCraftedLoot">Normally true but in rare cases when Eq is enhanced/fixed (e.g. Magical weapon recharge) false</param>
    /// <returns></returns>
    protected CraftingResult ReturnCraftedLoot(List<Loot> loot, List<Loot> usedSrcItems = null, bool deleteCraftedLoot = true)
    {
      return new CraftingResult(loot) { DeleteCraftedLoot = deleteCraftedLoot, UsedInputItems = usedSrcItems };
    }
  }
}
