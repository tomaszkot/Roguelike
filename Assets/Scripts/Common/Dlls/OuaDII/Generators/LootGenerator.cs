using Dungeons.Core;
using OuaDII.LootFactories;
using OuaDII.LootFactories.Equipment;
using OuaDII.Tiles.Looting;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Generators
{
  public class LootGenerator : Roguelike.Generators.LootGenerator
  {
    static Tiles.GodKind[] gods = new Tiles.GodKind[] { };
    static string[] godNames;
    HoundEqFactory houndEqFactory;

    static LootGenerator()
    {
      gods = Enum.GetValues(typeof(Tiles.GodKind)).Cast<Tiles.GodKind>().ToArray();
      godNames = gods.ToList().Select(i => i.ToString()).ToArray();
    }

    public LootGenerator(Container cont) : base(cont)
    {
      houndEqFactory = new HoundEqFactory(cont);
    }

    public static Tiles.GodKind GetGodKindFromName(string name)
    {
      try
      {
        return gods.Single(i => i.ToString() == name);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        throw;
      }
    }

    /// <summary>
    /// test 1111
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    public override Loot GetLootByAsset(string asset)
    {
      if (godNames.Contains(asset))
      {
        var god = new Tiles.Looting.GodStatue();
        god.GodKind = GetGodKindFromName(asset);
        return god;
      }

      var loot = LootFactory.GetByAsset(asset) as Roguelike.Tiles.Loot;
      if (loot == null)
      {
        loot = LootFactory.GetByAsset(asset.ToUpperFirstLetter()) as Roguelike.Tiles.Loot;
        if (loot == null)
        {
          loot = houndEqFactory.GetByAsset(asset);

          if (loot == null)
          {
            this.Container.GetInstance<ILogger>().LogError("GetLootByName failed for " + asset);
            return null;
          }
        }
      }
      PrepareLoot(loot);

      return loot;
    }

    protected override void PrepareLoot(Loot loot)
    {
      base.PrepareLoot(loot);
      if (loot is Equipment)
      {
        var eq = loot as Equipment;
        if (eq.LevelIndex <= 0)
          eq.SetLevelIndex(1);
      }
      //adjust price...
      loot.HandleGenerationDone();
    }

    List<RecipeKind> alrGenerated = new List<RecipeKind>();
    protected override Recipe GetRandRecipe()
    {
      var kind_ = RandHelper.GetRandomEnumValue<RecipeKind>();
      if (alrGenerated.Contains(kind_))
      {
        //give it a chance
        kind_ = RandHelper.GetRandomEnumValue<RecipeKind>();
      }
      
      if(!alrGenerated.Contains(kind_))
        alrGenerated.Add(kind_);
      return new Recipe(kind_);
    }
  }
}
