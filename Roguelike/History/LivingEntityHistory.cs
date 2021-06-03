using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.History
{
  public class LivingEntityHistoryItem : HistoryItem
  {
    public LivingEntityHistoryItem(LivingEntity dead)
    {
      Name = dead.name;
      Tag1 = dead.tag1;
      Herd = dead.Herd;
      if(dead is Enemy en)
        Power = en.PowerKind;
      Level = dead.Level;
    }

    public string Herd { get; set; }
    public Tiles.LivingEntities.EnemyPowerKind Power { get; set; }
    public int Level { get; set; }
  }

  public class LivingEntityHistory
  {
    public List<LivingEntityHistoryItem> Items { get; set; } = new List<LivingEntityHistoryItem>();

    public int CountByTag1(string tag1)
    {
      return Items.Where(i => i.Tag1 == tag1).Count();
    }

    public int CountByName(string name)
    {
      return Items.Where(i => i.Name == name).Count();
    }

    public int CountByHerd(string herd)
    {
      return Items.Where(i => i.Herd == herd).Count();
    }

    public void AddItem(LivingEntityHistoryItem lh)
    {
      Items.Add(lh);
    }

  }
}
