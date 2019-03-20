using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Roguelike.TileContainers
{
  //a giant node like 500x500 tiles
  public class World : GameNode
  {
    public override string Name { get; set; } = "";

    [JsonIgnore]//save by GameInfo
    public List<DungeonPit> Pits { get; set; } = new List<DungeonPit>();


    List<Stairs> pitsStairs = new List<Stairs>();
    const int defaultWidth = 15;
    const int defaultHeight = defaultWidth;

    public World() : this(defaultWidth, defaultHeight)
    {
      Name = "World1";
    }

    public World(int width, int height) : this(width, height, 
      new GenerationInfo()
      {
        RevealTiles = true,
        ChildIslandAllowed = false }//not supported now
      )
    {

    }
    public World(int width, int height, GenerationInfo gi, int index = 0) : base(width, height, gi, index)
    {

    }

    public override string ToString()
    {
      return GetType().Name + " " + GetHashCode() + " " + base.ToString();
    }

    public DungeonPit GetPit(string pitName)
    {
      var pit = Pits.Where(i => i.Name == pitName).FirstOrDefault();
      if (pit == null)
      {
        pit = AddPit(pitName);
      }

      return pit;
    }

    private DungeonPit AddPit(string pitName)
    {
      DungeonPit pit = new DungeonPit();
      pit.Name = pitName;
      Pits.Add(pit);
      return pit;
    }

    public override bool IsTileEmpty(Tile tile)
    {
      return tile.IsEmpty;
    }

    public override bool SetTile(Tile tile, Point point, bool resetOldTile = true, bool revealReseted = true)
    {
      if (tile is Tiles.Loot)
      {
        if (Loot.ContainsKey(point))// && loot[point] != null)
        {
          if (Logger != null)
            Logger.LogError("loot already at point: " + Loot[point] + ", trying to add: " + tile);
          return false;
        }
        tile.Point = point;
        Loot[point] = tile as Tiles.Loot;
        return true;
      }
      else
      {
        var newTile = base.SetTile(tile, point, resetOldTile, revealReseted);
        return newTile;
      }
    }

    public Tuple<World, List<Tile>> CreateDynamicWorld(World worldStatic)
    {
      
      var world = new World(worldStatic.Width, worldStatic.Height);
      world.AppendMaze(worldStatic);
      var dynTiles = this.GetTiles().Where(i => i.IsDynamic()).ToList();

      foreach (var tile in dynTiles)
      {
        world.SetTile(tile, tile.Point);
      }

      return new Tuple<World, List<Tile>>(world, dynTiles);
    }

    public void AddStairsWithPit(string pitName, Point point)
    {
      var stairs = new Stairs(StairsKind.PitDown);
      stairs.PitName = pitName;
      SetTile(stairs, point);
      AddPit(pitName);
    }

    public override Tile GetTile(Point point)
    {
      var tile = base.GetTile(point);
      if (tile == null || tile.IsEmpty)
      {
        var lootTile = GetLootTile(point);
        return lootTile != null ? lootTile : tile;
      }

      return tile;
    }

    public override string Description
    {
      get {
        var desc =  "World " + Name + " " + GetHashCode();
        
        return desc;

      }
    }
    
    //not sure I want it
    //public override Tile GenerateEmptyTile(Point pt)
    //{
    //  return null;
    //}

    //not sure I want it
    protected override Point GetEmptyNeighborhoodPoint(Tile target, List<TileNeighborhood> sides)
    {
      Point pt = new Point().Invalid();
      foreach (var side in sides)
      {
        var tile = GetNeighborTile(target, side);
        if (IsTileEmpty(tile))
        {
          pt = base.GetNeighborPoint(target, side);
          if (base.IsPointInBoundaries(pt))
            return pt;
        }
      }

      return pt;
    }

    internal GameNode PlaceLoadedHero(Hero hero, GameState gs)
    {
      GameNode node = this;
      if (!string.IsNullOrEmpty(gs.HeroPathValue.Pit))
      {
        var pit = Pits.Where(i => i.Name == gs.HeroPathValue.Pit).SingleOrDefault();
        node = pit.Levels[gs.HeroPathValue.LevelIndex];
      }
      if (!node.SetTile(hero, hero.Point))
        Logger.LogError("failed to set hero at " + hero.Point);
      return node;

    }

    internal override bool RevealRoom(Tiles.Door door, Roguelike.Tiles.Hero hero)
    {
      door.Opened = true;
      return true;
    }
  }
}
