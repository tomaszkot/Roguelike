#define ASCII_BUILD
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract;
using Roguelike.Abstract.Inventory;
using Roguelike.Abstract.Tiles;
using Roguelike.LootContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Tiles.Interactive
{
  //a chest that hero can put his private stuff to
  public class HeroChest : Roguelike.Tiles.Interactive.InteractiveTile, IInventoryOwner
  {
    public HeroChest(Container cont) : base(cont, '~')
    {
#if ASCII_BUILD
      color = ConsoleColor.Red;
#endif
      tag1 = "hero_chest";
      Revealed = true;

      Kind = Roguelike.Tiles.Interactive.InteractiveTileKind.Unset;
    }

    [JsonIgnore]//hero will persist it
    public Inventory Inventory
    {
      get { return Hero.Chest.Inventory; }
    }

    [JsonIgnore]
    public OuaDII.Tiles.LivingEntities.Hero Hero
    {
      get;
      set;
    }

    public int Gold { get; set; }


    public int GetPrice(Loot loot)
    {
      return loot.Price;
    }

    public bool GetGoldWhenSellingTo(IInventoryOwner dest)
    {
      return false;
    }
  }
}
