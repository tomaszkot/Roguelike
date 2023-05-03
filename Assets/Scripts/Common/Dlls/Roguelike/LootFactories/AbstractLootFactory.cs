using Dungeons.Core;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.LootFactories
{
  public abstract class AbstractLootFactory
  {
    protected T GetRandom<T>(Dictionary<string, Func<string, T>> factory)
    {
      var index = RandHelper.GetRandomInt(factory.Count);
      var lootCreator = factory.ElementAt(index);
      return lootCreator.Value(lootCreator.Key);
    }

    protected abstract void Create();
    public abstract Loot GetRandom(int level);// where T : Loot;
    public abstract Loot GetByName(string name);
    public abstract Loot GetByAsset(string tagPart);
    protected Container container;

    public AbstractLootFactory(Container container)
    {
      this.container = container;
      Create();
    }

    public virtual IEnumerable<Loot> GetAll()
    {
      return new List<Loot>();
    }
  }

  public abstract class EquipmentTypeFactory : AbstractLootFactory
  {
    protected Dictionary<string, Func<string, Equipment>> factory = new Dictionary<string, Func<string, Equipment>>();
    protected Dictionary<string, Roguelike.Tiles.Looting.Equipment> prototypes = new Dictionary<string, Roguelike.Tiles.Looting.Equipment>();

    public EquipmentTypeFactory(Container container) : base(container)
    {
    }
    protected void CreatePrototypes()
    {
      foreach (var sh in factory.Keys)
      {
        if (factory.ContainsKey(sh))
        {
          var res = factory[sh](sh);
          prototypes.Add(sh, res);
        }
        else
          ReportError("!factory.ContainsKey(sh): " + sh);
      }
    }

    protected void ReportError(string error)
    {
      container.GetInstance<ILogger>().LogError(error);
    }

    public List<Roguelike.Tiles.Looting.Equipment> GetUniqueItems(int level)
    {
      return prototypes.Where(i => i.Value.Class == Roguelike.Tiles.EquipmentClass.Unique && i.Value.LevelIndex <= level).Select(i => i.Value).ToList();
    }

    public override Loot GetRandom(int level)
    {
      var lootProto = GetRandromFromPrototype(level);
      if (lootProto != null)
        return lootProto;

      Equipment loot = GetRandomFromAll();

      return loot;
    }

    protected Equipment GetRandomFromAll()
    {
      var index = RandHelper.GetRandomInt(factory.Count);
      var lootCreator = factory.ElementAt(index);
      var loot = lootCreator.Value(lootCreator.Key);
      return loot;
    }

    protected void EnsureMaterialFromLootSource(Equipment eq, EquipmentMaterial mat)
    {
      if (!eq.IsMaterialAware())
        return;
      // int level = eq.LevelIndex;
      //if (level > MaterialProps.IronDropLootSrcLevel)
      {
        eq.SetMaterial(mat);
      }
    }

    protected virtual Loot GetRandromFromPrototype(int level)
    {
      if (prototypes.Any())
      {
        var eq = GetLootAtLevel(level);
        if (eq.Item2 == EquipmentMaterial.Bronze && level > 7)
        {
         // if (LevelIndex > 7)
          {
            int k = 0;
            k++;
          }
        }
        return CreateItem(level, eq);
      }
      return null;
    }

    protected Loot CreateItem(int level, Tuple<string, EquipmentMaterial> eqDesc)
    {
      if (eqDesc != null && eqDesc.Item1.Any())
      {
        var eqCreator = factory[eqDesc.Item1];
        var eqDone = eqCreator(eqDesc.Item1);
        if (eqDesc.Item2 != EquipmentMaterial.Unset)
          EnsureMaterialFromLootSource(eqDone, eqDesc.Item2);

        if (eqDone is Weapon wpn && wpn.IsMagician && wpn.Class != EquipmentClass.Unique)
        {
          wpn.SetLevelIndex(level);//TODO
        }
        return eqDone;
      }
     
      return null;
    }

    List<Equipment> GetPlainsAtLevel(int level, bool materialAware, ref bool fakeDecreaseOfLevel)
    {
      var plains = prototypes.Values.Where(i => i.Class == EquipmentClass.Plain).ToList();

      var plainsAtLevel = GetPlainsAtLevel(level, materialAware, plains);
      if (level > 1)
      {
        fakeDecreaseOfLevel = false;
        while (!plainsAtLevel.Any())// &&  && RandHelper.GetRandomDouble() < 0.5f)
        {
          level--;
          plainsAtLevel = GetPlainsAtLevel(level, materialAware, plains);
          fakeDecreaseOfLevel = true;
        }
      }
      return plainsAtLevel;
    }

    private static List<Equipment> GetPlainsAtLevel(int level, bool materialAware, List<Equipment> plains)
    {
      var plainsAtLevel = plains.Where(i => i.LevelIndex == level).ToList();
      if (materialAware)
        plainsAtLevel = plainsAtLevel.Where(i => i.IsMaterialAware()).ToList();
      return plainsAtLevel;
    }

    protected virtual Tuple<string, EquipmentMaterial> GetLootAtLevel(int level)
    {
      EquipmentMaterial mat = EquipmentMaterial.Unset;
      bool fakeDecreaseOfLevel = false;
      var plainsAtLevel = GetPlainsAtLevel(level, false, ref fakeDecreaseOfLevel);
      if (level > 1)
        plainsAtLevel.AddRange(GetPlainsAtLevel(level-1, false, ref fakeDecreaseOfLevel));//give a chance to get material aware
      var eq = plainsAtLevel.GetRandomElem();
      var orgLevel = level;
      if (eq.Name == "Hammer")
      {
        int k = 0;
        k++;
      }
      //Debug.WriteLine("GetLootAtLevel "+ eq + " start");
      if (eq.IsMaterialAware())
      {
        GetMaterial(ref level, ref mat, fakeDecreaseOfLevel);
        //Debug.WriteLine("GetMaterial " + mat + " level != orgLevel "+ (level != orgLevel));
        //if (level != orgLevel)
        //{
        //  var newEq = plains.Where(i => i.LevelIndex == level).Where(i => i.IsMaterialAware()).ToList().GetRandomElem();
        //  if (newEq == null)
        //  {
        //    mat = EquipmentMaterial.Unset;
        //    level = orgLevel;
          
        //    GetMaterial(ref level, ref mat, fakeDecreaseOfLevel);
        //    Debug.WriteLine("GetMaterial rewert!");
        //  }
        //  else
        //  {
        //    eq = newEq;
        //  }
        //}
      }


     // Debug.WriteLine("GetLootAtLevel " + eq + " end");
      return ReturnEq(mat, eq);
    }

    private static void GetMaterial(ref int level, ref EquipmentMaterial mat, bool fakeDecreaseOfLevel)
    {

      //bool upgMaterial = false;
      float nextMatThreshold = 0.4f;
      var lvl = level;
      if (fakeDecreaseOfLevel)
        lvl++;
      if (lvl > MaterialProps.SteelDropLootSrcLevel)
      {
        mat = EquipmentMaterial.Iron;
        if (RandHelper.GetRandomDouble() > nextMatThreshold)//make it more unpredictable 
        {
          mat = EquipmentMaterial.Steel;
         //upgMaterial = true;
        }
      }
      else if (lvl > MaterialProps.IronDropLootSrcLevel)
      {
        if (RandHelper.GetRandomDouble() > nextMatThreshold)
        {
          mat = EquipmentMaterial.Iron;
          //upgMaterial = true;
        }
      }
      //TODO ? too complicated..
      //if (upgMaterial)
      //  level--;
    }

    protected Tuple<string, EquipmentMaterial> GetRandomFromList(int level, EquipmentMaterial mat, List<Equipment> plains)
    {
      
      var eqsAtLevel = plains.Where(i => i.LevelIndex == level).ToList();

      var eq = eqsAtLevel.GetRandomElem();
      return ReturnEq(mat, eq);
    }

    private static Tuple<string, EquipmentMaterial> ReturnEq(EquipmentMaterial mat, Equipment eq)
    {
      Tuple<string, EquipmentMaterial> res = null;
      if (eq != null)
      {
        if (eq.IsMaterialAware())
        {
          if (mat == EquipmentMaterial.Unset)
          {
            mat = EquipmentMaterial.Bronze;
          }
        }
        res = new Tuple<string, EquipmentMaterial>(eq.tag1, mat);
      }

      return res;
    }

    public override Loot GetByName(string name)
    {
      return GetByAsset(name);
    }

    public override Loot GetByAsset(string tagPart)
    {
      var tile = factory.FirstOrDefault(i => i.Key.ToLower() == tagPart.ToLower());
      if (tile.Key != null)
        return tile.Value(tagPart);

      return null;
    }

    public override IEnumerable<Loot> GetAll()
    {
      List<Loot> loot = new List<Loot>();
      foreach (var lc in factory)
        loot.Add(lc.Value(lc.Key));

      return loot;
    }

  }}
