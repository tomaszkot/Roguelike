using Dungeons.Core;
using Roguelike.Extensions;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.Crafting
{
  public class RecipeUsingAdvisor : LootCrafterTools
  {
    Container container;
    WorkersFactory wf;
    ILootCrafter lootCrafter;

    public RecipeUsingAdvisor(Container container, ILootCrafter lootCrafter)
    {
      this.container = container;
      this.lootCrafter = lootCrafter;
      wf = new WorkersFactory(new List<Loot> { }, lootCrafter, container);
    }

    //public RecipeKind[] GetPossibleKinds(Hero hero)
    //{
    //  var recipeKinds = EnumHelper.GetEnumValues<RecipeKind>(true);
    //  recipeKinds.Remove(RecipeKind.Custom);
    //  var recipeKindsToUse = recipeKinds.ToList();
    //  foreach (var rk in recipeKinds)
    //  { 
    //    if(!CanUseRecipe(rk, hero))
    //      recipeKindsToUse.Remove(RecipeKind.Custom);
    //  }
    //  return recipeKindsToUse.ToArray();
    //}

    public bool CanCopyLootToCraftPanel(RecipeKind rk, Hero hero)
    {
      CraftWorker worker = GetWorker(rk);
      if(worker == null) 
        return false;

      bool forCopyLootToCraftPanel = true;
      if (GetRecipeRequiredLoot(rk, hero, CraftWorker.IsAdvisorSupportsBulk(rk), forCopyLootToCraftPanel).Any())
      {
        return true;
      }

      return false;
    }

    public bool CanUseRecipe(RecipeKind rk, Hero hero)
    {
      CraftWorker worker = GetWorker(rk);
      if (worker == null)
        return false;

      bool forCopyLootToCraftPanel = false;
      if (GetRecipeRequiredLoot(rk, hero, CraftWorker.IsAdvisorSupportsBulk(rk), forCopyLootToCraftPanel).Any())
      {
        return true;
      }

      return false;
    }

    public List<Loot> GetRecipeRequiredLoot(RecipeKind rk, Hero hero, bool bulkCraft, bool forCopyLootToCraftPanel)
    {
      var worker = GetWorker(rk);
      if (worker == null)
        return new List<Loot>();
     
      var invItems = forCopyLootToCraftPanel ? hero.Inventory.Items : hero.Crafting.InvItems.Inventory.Items;
      List<Loot> res = new List<Loot>();

      if (rk == RecipeKind.TransformPotion ||
          rk == RecipeKind.Toadstools2Potion)
      {
        var canDoRes = CanRunWorker(worker, invItems);
        if (canDoRes.Success)
          res = canDoRes.UsedInputItems;
      }
      else
      {
        var requiredItem = worker.GetRequiredItems();
        res = GetLoot(rk, bulkCraft, worker, invItems, requiredItem);
        var mightMatch = res.Count >= requiredItem.Items.Count;
        if (mightMatch)
        {
          if (!CanRunWorker(worker, res).Success)
            res.Clear();
        }
        else
          res.Clear();
      }

      if (res.Any())
      {
        var recipe = new Recipe(worker.Kind);
        var md = FilterOne<MagicDust>(hero.Crafting.InvItems.Inventory.Items);
        if (md == null || md.Count < recipe.MagicDustRequired)
        {
          //var md = FilterOne<MagicDust>(hero.Crafting.InvItems.Inventory.Items);
          res.Clear();
          Debug.WriteLine("recipe "+ rk+ " due to lack of md!");
        }
        else
          res.Add(md);
      }
      return res;
    }

    private List<Loot> GetLoot
    (
      RecipeKind rk, bool bulkCraft, CraftWorker worker, List<Loot> invItems,
      RecipeRequiredItems requiredItems
    )
    {
      var res = new List<Loot>();
      foreach (var nextItemsDesc in requiredItems.Items)
      {
        var invLootItems = invItems.Where(i =>
        (i.GetType() == nextItemsDesc.Type || i.GetType().IsSubclassOf(nextItemsDesc.Type)) &&
        !res.Contains(i) &&
        i.IsMatchingRecipe(rk)
        ).ToList();
        if (invLootItems.Any())
        {
          if (!bulkCraft)
          {
            if (nextItemsDesc.Type == typeof(Equipment))
            {
              worker.Init(invLootItems, lootCrafter);
              var eq = worker.Eqs.FirstOrDefault();
              if (eq != null)
                res.Add(eq);
            }
            else
            {
              var loot = invLootItems.First();
              res.Add(loot);
              //bug!!!!
              //if (loot is StackedLoot sl)
              //{
              //  sl.Count = nextItemsDesc.MinCount;
              //}
            }
          }
          else
          {
            var toAdd = invLootItems;
            if (nextItemsDesc.Type == typeof(Equipment))
            {
              if (rk == RecipeKind.OneEq || rk == RecipeKind.TwoEq)
              {
                toAdd = Filter<Equipment>(toAdd).Where(i => i.Class != EquipmentClass.Unique).Cast<Loot>().ToList();
              }
            }
            res.AddRange(invLootItems);
          }
        }
        else
          Debug.WriteLine("Not found inv item for " + nextItemsDesc);
      }

      return res;
    }

    public CraftWorker GetWorker(RecipeKind rk)
    {
      return wf.GetWorker(rk);
    }

    private void PrepareForTranform(List<Loot> invItems, List<Loot> res)
    {
      var pot = res.Where(i => i.IsPotion()).FirstOrDefault() as Potion;
      var mush = FilterOne<Mushroom>(res);
      if (pot != null)
      {
        Potion otherPot = null;
        if (pot.Kind == PotionKind.Health || (mush != null && mush.MushroomKind == MushroomKind.RedToadstool))
        {
          otherPot = invItems.Where(i => i.IsPotionKind(PotionKind.Mana)).FirstOrDefault().AsPotion();

        }
        else if (pot.Kind == PotionKind.Mana || (mush != null && mush.MushroomKind == MushroomKind.BlueToadstool))
        {
          otherPot = invItems.Where(i => i.IsPotionKind(PotionKind.Health)).FirstOrDefault().AsPotion();

        }
        if (otherPot != null)
        {
          res.Remove(pot);
          res.Add(otherPot);
        }
      }
    }

    private CraftingResult CanRunWorker(CraftWorker worker, List<Loot> res)
    {
      worker.Init(res, lootCrafter);
      return worker.CanDo();
    }
  }
}
