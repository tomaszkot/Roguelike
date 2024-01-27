using Dungeons.Core;
using Roguelike.LootFactories;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using System.Collections.Generic;
using System;
using SimpleInjector;
using System.Linq;

public abstract class EquipmentTypeFactory : AbstractLootFactory
{
  protected Dictionary<string, Func<string, Equipment>> factory = new Dictionary<string, Func<string, Equipment>>();

  protected Dictionary<string, PrototypeValue> prototypesPlain = new Dictionary<string, PrototypeValue>();
  protected Dictionary<string, PrototypeValue> prototypesUnique = new Dictionary<string, PrototypeValue>();

  public EquipmentTypeFactory(Container container) : base(container)
  {
  }

  protected void CreatePrototypes()
  {
    foreach (var tag in factory.Keys)
    {
      if (factory.ContainsKey(tag))
      {
        var res = factory[tag](tag);

        var pv = GetFromEq(res);
        if (res.Class == EquipmentClass.Plain)
          prototypesPlain.Add(tag, pv);
        if (res.Class == EquipmentClass.Unique)
          prototypesUnique.Add(tag, pv);
      }
      else
        ReportError("!factory.ContainsKey(tag): " + tag);
    }
  }

  protected PrototypeValue GetFromEq(Equipment res)
  {
    var pv = new PrototypeValue()
    {
      Level = res.LevelIndex,
      IsMaterialAware = res.IsMaterialAware(),
      Material = res.Material,
      Tag = res.tag1,
      Kind = res.EquipmentKind
    };
    if (res is Weapon wpn)
      pv.WeaponKind = wpn.Kind;

    return pv;
  }

  protected void ReportError(string error)
  {
    container.GetInstance<ILogger>().LogError(error);
  }

  public List<string> GetUniqueItems(int level)
  {
    return prototypesUnique.Where(i => i.Value.Level <= level).Select(i => i.Key).ToList();
  }

  public List<string> GetNotUniqueItems(int level)
  {
    return prototypesPlain.Where(i => i.Value.Level <= level).Select(i => i.Key).ToList();
  }

  public override Loot GetRandom(int level)
  {
    var lootProto = GetRandromFromPrototype(level);
    if (lootProto != null)
      return lootProto;

    //Equipment loot = GetRandomFromAll();-returned uniq

    return null;
  }


  protected void EnsureMaterialFromLootSource(Equipment eq, EquipmentMaterial mat)
  {
    if (!eq.IsMaterialAware())
      return;
    {
      eq.SetMaterial(mat);
    }
  }

  protected virtual Loot GetRandromFromPrototype(int level)
  {
    var eq = GetLootAtLevel(level);
    if (eq == null) return null;

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

  public List<PrototypeValue> GetPlainsAtLevel(int level, bool materialAware, bool alwaysTwo, ref bool fakeDecreaseOfLevel)
  {
    //var plains = prototypesPlain.Keys.ToList();

    var plainsAtLevel = GetPlainsAtLevel(level, materialAware);
    if (level > 1)
    {
      fakeDecreaseOfLevel = false;
      while (!plainsAtLevel.Any() || alwaysTwo)// &&  && RandHelper.GetRandomDouble() < 0.5f)
      {
        level--;
        plainsAtLevel.AddRange(GetPlainsAtLevel(level, materialAware));
        fakeDecreaseOfLevel = true;
        if (plainsAtLevel.Count > 1)
          break;
      }
    }
    return plainsAtLevel;
  }

  private List<PrototypeValue> GetPlainsAtLevel(int level, bool materialAware)
  {
    var plainsAtLevel = prototypesPlain.Where(i => i.Value.Level == level).Select(i => i.Value).ToList();
    if (materialAware)
      plainsAtLevel = plainsAtLevel.Where(i => i.IsMaterialAware).ToList();
    return plainsAtLevel;
  }

  protected virtual Tuple<string, EquipmentMaterial> GetLootAtLevel(int level)
  {
    EquipmentMaterial mat = EquipmentMaterial.Unset;
    bool fakeDecreaseOfLevel = false;
    var plainsAtLevel = GetPlainsAtLevel(level, false, alwaysTwo: false, ref fakeDecreaseOfLevel);
    if (level > 1)
      plainsAtLevel.AddRange(GetPlainsAtLevel(level - 1, false, alwaysTwo: false, ref fakeDecreaseOfLevel));//give a chance to get material aware
    var eq = plainsAtLevel.GetRandomElem();
    var orgLevel = level;
    //if (eq.Name == "Hammer")
    //{
    //  int k = 0;
    //  k++;
    //}
    //Debug.WriteLine("GetLootAtLevel "+ eq + " start");
    if (eq.IsMaterialAware)
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
    return ReturnEqDescription(mat, eq);
  }

  protected static void GetMaterial(ref int level, ref EquipmentMaterial mat, bool fakeDecreaseOfLevel)
  {
    var lvl = level;
    if (fakeDecreaseOfLevel)
      lvl++;
    mat = GetMaterial(mat, lvl);
    //TODO ? too complicated..
    //if (upgMaterial)
    //  level--;
  }
  protected static EquipmentMaterial GetMaterial(int lvl)
  {
    EquipmentMaterial mat = EquipmentMaterial.Bronze;
    return GetMaterial(mat, lvl);
  }


  private static EquipmentMaterial GetMaterial(EquipmentMaterial mat, int lvl)
  {
    float nextMatThreshold = 0.4f;
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

    return mat;
  }

  protected Tuple<string, EquipmentMaterial> GetRandomFromList(int level, EquipmentMaterial mat, List<PrototypeValue> plains)
  {
    PrototypeValue eq = GetRandFromList(level, plains);
    return ReturnEqDescription(mat, eq);
  }

  private PrototypeValue GetRandFromList(int level, List<PrototypeValue> plains)
  {
    var eqsAtLevel = plains.Where(i => i.Level == level).ToList();
    var eq = eqsAtLevel.GetRandomElem();
    return eq;
  }

  protected Tuple<string, EquipmentMaterial> ReturnEqDescription(EquipmentMaterial mat, PrototypeValue eq)
  {
    Tuple<string, EquipmentMaterial> res = null;
    if (eq != null)
    {
      if (eq.IsMaterialAware)
      {
        if (mat == EquipmentMaterial.Unset)
        {
          mat = EquipmentMaterial.Bronze;
        }
      }
      res = new Tuple<string, EquipmentMaterial>(eq.Tag, mat);
    }

    return res;
  }

  //public override Loot GetByName(string name)
  //{
  //  return GetByAsset(name);
  //}

  public override Loot GetByAsset(string tagPart)
  {
    return GetByAsset(factory, tagPart);
  }

  public override IEnumerable<Loot> GetAll()
  {
    var loot = new List<Loot>();
    foreach (var lc in factory)
    {
      var lootItem = GetByAsset(lc.Key);
      loot.Add(lootItem);
    }

    return loot;
  }



}
