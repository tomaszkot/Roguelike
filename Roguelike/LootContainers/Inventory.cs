using System.Collections.Generic;
using System.Linq;
using Roguelike.Tiles;
using Dungeons.ASCIIDisplay.Presenters;
using Dungeons.ASCIIDisplay;
using Roguelike.Managers;
using Roguelike.Events;
using Newtonsoft.Json;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;

namespace Roguelike.LootContainers
{
  public class Inventory : InventoryBase
  {
    public Inventory() : base()
    {
      
    }
  }
}
