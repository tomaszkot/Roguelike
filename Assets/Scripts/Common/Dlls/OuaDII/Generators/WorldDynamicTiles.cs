using Roguelike.Tiles.Interactive;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelike.Tiles.LivingEntities;
using OuaDII.TileContainers;
using Roguelike.Tiles.Looting;

namespace OuaDII.Generators
{
  public class WorldDynamicTiles
  {
    public List<Chest> Chests = new List<Chest>();
    public List<Enemy> Enemies = new List<Enemy>();
    public List<DeadBody> DeadBodies = new List<DeadBody>();
    public List<Barrel> Barrels = new List<Barrel>();
    public List<Mushroom> Mushrooms = new List<Mushroom>();
    public List<Food> Food = new List<Food>();
    public List<MagicDust> MagicDusts = new List<MagicDust>();

    public List<Plant> Plants = new List<Plant>();
    public List<Roguelike.Tiles.Loot> OtherLoot = new List<Loot>();
    public List<Animal> Animals = new List<Animal>();
  }
}
