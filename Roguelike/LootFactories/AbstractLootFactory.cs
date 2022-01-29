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
    protected Dictionary<string, Roguelike.Tiles.Equipment> prototypes = new Dictionary<string, Roguelike.Tiles.Equipment>();

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

    public List<Roguelike.Tiles.Equipment> GetUniqueItems(int level)
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
          wpn.LevelIndex = level;
        }
        return eqDone;
      }
     
      return null;
    }

    protected virtual Tuple<string, EquipmentMaterial> GetLootAtLevel(int level)
    {
      Tuple<string, EquipmentMaterial> res = new Tuple<string, EquipmentMaterial>("", EquipmentMaterial.Unset);
      EquipmentMaterial mat = EquipmentMaterial.Unset;
      bool upgMaterial = false;
      if (level > MaterialProps.SteelDropLootSrcLevel)
      {
        mat = EquipmentMaterial.Iron;
        if (RandHelper.GetRandomDouble() > 0.33f)//make it more unpredictable 
        {
          mat = EquipmentMaterial.Steel;
          upgMaterial = true;
        }
      }
      else if (level > MaterialProps.IronDropLootSrcLevel)
      {
        if (RandHelper.GetRandomDouble() > 0.33f)
        {
          mat = EquipmentMaterial.Iron;
          upgMaterial = true;
        }
      }

      if (upgMaterial)
        level--;

      var plains = prototypes.Values.Where(i => i.Class == EquipmentClass.Plain).ToList();
      res = GetRandomFromList(level, ref mat, plains);
      //if (eq !=null mat != EquipmentMaterial.Bronze)
      //{
      //  EnsureMaterialFromLootSource(eq);
      //}
      return res;
    }
    protected Tuple<string, EquipmentMaterial> GetRandomFromList(int level, ref EquipmentMaterial mat, List<Equipment> plains)
    {
      Tuple<string, EquipmentMaterial> res = null;
      var eqsAtLevel = plains.Where(i => i.LevelIndex == level).ToList();

      var eq = RandHelper.GetRandomElem<Equipment>(eqsAtLevel);
      if (eq != null)
      {
        if (eq.IsMaterialAware() && mat == EquipmentMaterial.Unset)
          mat = EquipmentMaterial.Bronze;
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
