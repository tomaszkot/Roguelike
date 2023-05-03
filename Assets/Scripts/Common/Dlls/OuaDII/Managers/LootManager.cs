//using OuaDII.Tiles.LivingEntities;
using OuaDII.Tiles;
using OuaDII.Tiles.Looting;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Managers
{
  public class LootManager : Roguelike.Managers.LootManager
  {
    Dictionary<string, OuaDII.Tiles.GodKind> godKinds = new Dictionary<string, Tiles.GodKind>()
    {
      { "rat_boss", OuaDII.Tiles.GodKind.Wales },
      { "bat_boss", OuaDII.Tiles.GodKind.Jarowit },
      { "skeleton_boss", OuaDII.Tiles.GodKind.Swiatowit },
      { "spider_boss", OuaDII.Tiles.GodKind.Swarog }
      ,{ "worm_boss", OuaDII.Tiles.GodKind.Dziewanna }
      ,{ "snake_boss", OuaDII.Tiles.GodKind.Perun }
    };

    public Dictionary<string, GodKind> GodKinds { get => godKinds; }

    protected override List<Loot> GetExtraLoot(ILootSource lootSource, Loot primaryLoot)
    {
      var loot = base.GetExtraLoot(lootSource, primaryLoot);
      if (lootSource is Enemy en)
      {
        if (en.PowerKind == EnemyPowerKind.Boss)
        {
          var statue = GetSlavicGodStatue(en);
          if (statue != null)
          {
            var hasStatue = false;
            var godInEq = this.GameManager.Hero.CurrentEquipment.GetActiveEquipment(CurrentEquipmentKind.God) as Roguelike.Tiles.Looting.Equipment;
            if (godInEq != null && godInEq.tag1 == statue.tag1)
              hasStatue = true;
            else
            {
              if(this.GameManager.Hero.Inventory.Items.Any(i => i.tag1 == statue.tag1))
                hasStatue = true;
            }
            if (!hasStatue)
              loot.Add(statue);
          }

          loot.Add(LootGenerator.GetRandomLoot(LootKind.Book, 1));
        }
        
      }
      return loot;
    }

    private GodStatue GetSlavicGodStatue(Enemy en)
    {
      if (godKinds.ContainsKey(en.tag1))
      {
        var stat = new GodStatue();
        stat.GodKind = godKinds[en.tag1];
        return stat;
      }

      return null;
    }
  }
}
