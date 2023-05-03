//using Dungeons;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using OuaDII.Generators;
using OuaDII.TileContainers;
using OuaDII.Tiles.Interactive;
using Roguelike.Serialization;
using Roguelike.Settings;
using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.IO;

namespace OuaDII.Serialization
{

  class WorldSaveData : IPersistable
  {
    public List<Roguelike.Tiles.Interactive.InteractiveTile> InteractiveTiles { get; set; }
    public List<Roguelike.Tiles.LivingEntities.LivingEntity> LivingEntitiesTiles { get; set; }
    public List<Roguelike.Tiles.Loot> LootTiles { get; set; }

    public WorldSpecialTiles WorldSpecialTiles { get; set; }
    public Dungeons.HiddenTiles HiddenTiles { get; set; } = new Dungeons.HiddenTiles();

    public int WorldWidth { get; set; }
    public int WorldHeight { get; set; }

    public void FromWorld(World world)
    {
      InteractiveTiles = world.GetTiles<Roguelike.Tiles.Interactive.InteractiveTile>();
      LivingEntitiesTiles = world.GetTiles<Roguelike.Tiles.LivingEntities.LivingEntity>();
      LootTiles = world.GetTiles<Roguelike.Tiles.Loot>();

      WorldWidth = world.Width;
      WorldHeight = world.Height;
      WorldSpecialTiles = world.WorldSpecialTiles;
      HiddenTiles = world.HiddenTiles;
    }

    public void ToWorld(World world)
    {
      GenerationInfo info = new GenerationInfo();
      info.MakeEmpty();
      world.Create(WorldWidth, WorldHeight, info);
      AppendToWorld(world);
    }

    public void AppendToWorld(World world)
    {
      SetWorldTile(world, InteractiveTiles);
      SetWorldTile(world, LivingEntitiesTiles);
      SetWorldTile(world, LootTiles);
      world.WorldSpecialTiles = WorldSpecialTiles;
      world.HiddenTiles = HiddenTiles;
    }

    private void SetWorldTile<T>(World world, List<T> tiles) where T : Dungeons.Tiles.Tile
    {
      foreach (var tile in tiles)
      {
        try
        {
          world.SetTile(tile, tile.point);
        }
        catch (Exception)
        {
        }
      }
    }
  }

  public class JSONPersister : Roguelike.Serialization.JSONPersister, IPersister
  {
    const string PitFolder = "Pits";

    protected enum OuadFileKind { Hero, GameLevel, GameState }

    protected override string GameLevelsFolder { get { return PitFolder; } }

    public JSONPersister(Container container) : base(container)
    {

    }

    public string GetGameFolder(string heroName, bool quick)
    {
      return GetFilesPath(heroName, quick);
    }

    public string GetWorldFilePath(string heroName, bool quick)
    {
      return Path.Combine(new[] { GetGameFolder(heroName, quick), "World" + extension });
    }

    public void SaveWorld(string heroName, World world, bool quick)
    {
      var worldSaveData = new WorldSaveData();
      worldSaveData.FromWorld(world);
      Save<WorldSaveData>(worldSaveData, GetWorldFilePath(heroName, quick));
    }

    public World LoadWorld(string heroName, bool quick)
    {
      var worldData = Load<WorldSaveData>(GetWorldFilePath(heroName, quick), Container);
      World world = new World(Container);
      worldData.ToWorld(world);
      return world;
    }

    public void LoadWorld(string heroName, bool quick, World world)
    {
      var worldData = Load<WorldSaveData>(GetWorldFilePath(heroName, quick), Container);
      worldData.AppendToWorld(world);
    }

    public void SavePits(string heroName, List<DungeonPit> pits, bool quick)
    {
      pits.ForEach(i =>
      {
        var pitFullPath = GetFullFilePath(FileKind.GameLevel, heroName, quick, i.Name);
        Save<DungeonPit>(i, pitFullPath);
      });
    }

    public override void DeleteGame(string heroName, bool quick)
    {
      var folderPath = GetGameFolder(heroName, quick);
      var tmpPath = Path.GetTempPath();
      var destPath = Path.Combine(tmpPath, heroName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
      if (Directory.Exists(destPath))
        destPath += Guid.NewGuid();
      Directory.Move(folderPath, destPath);
    }

    public List<DungeonPit> LoadPits(string heroName, bool quick)
    {
      var pits = new List<DungeonPit>();
      var pitDirFull = Path.Combine(GetGameFolder(heroName, quick), GameLevelsFolder);
      if (Directory.Exists(pitDirFull))
      {
        var files = Directory.GetFiles(pitDirFull, "*");
        foreach (var pitFileName in files)
          pits.Add(Load<DungeonPit>(pitFileName, Container));
      }
      return pits;
    }

    public void SaveTestingData(TestingData td)
    {
      var filePath = Path.Combine(GamePath, "TestingData" + extension);
      Save<TestingData>(td, filePath);
    }
    public void DeleteTestingData()
    {
      var filePath = Path.Combine(GamePath, "TestingData" + extension);
      try
      {
        if (File.Exists(filePath))
          File.Delete(filePath);
      }
      catch (Exception ex)
      {
        Container.GetInstance<Dungeons.Core.ILogger>().LogError(ex);
      }
    }


    public TestingData LoadTestingData()
    {
      var filePath = Path.Combine(GamePath, "TestingData" + extension);
      if (File.Exists(filePath))
      {
        try
        {
          return Load<TestingData>(filePath, Container);
        }
        catch (Exception ex)
        {
          Container.GetInstance<Dungeons.Core.ILogger>().LogError(ex);
        }
      }
      return null;
    }
  }
}