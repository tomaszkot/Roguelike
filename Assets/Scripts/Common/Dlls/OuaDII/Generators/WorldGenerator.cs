using Dungeons.Core;
using Dungeons.Tiles;
using OuaDII.Discussions;
using OuaDII.Managers;
using OuaDII.Quests;
using OuaDII.TileContainers;
using OuaDII.Tiles.Interactive;
using Roguelike.TileContainers;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OuaDII.Generators
{
  public interface IWorldGenerator
  {
  }

  public class WorldGenerator : Dungeons.DungeonGenerator, IWorldGenerator
  {
    public const string PitRats = "pit_down_rats";
    public const string PitBats = "pit_down_bats";
    public const string PitSpiders = "pit_down_spiders";
    public const string PitSkeletons = "pit_down_skeletons";
    public const string PitSnakes = "pit_down_snakes";
    public const string PitWorms = "pit_down_worms";
    World world;
    OuaDII.Generators.GenerationInfo gi;
    
    public static string[] PrimaryPits = new[] { PitRats, PitBats, PitSpiders, PitSkeletons, PitSnakes, PitWorms };

    public static string[] Pits;

    World worldNode;
    public Roguelike.Tiles.LivingEntities.Hero Hero { get; set; }
    public World World { get => worldNode; set => worldNode = value; }
    public bool GenerateDynamicTiles { get; set; }
    GameManager gm;
    Roguelike.Tiles.LivingEntities.Hero hero;

    public WorldGenerator(Container container, GameManager gm) : base(container)
    {
      this.gm = gm;
    }

    static WorldGenerator()
    {
      List<string> pits = new List<string>();// PrimaryPits.ToList();

      pits.AddRange
      (
        new[]
        { 
          DungeonPit.GetPitQuestName(QuestKind.Smiths),
          DungeonPit.GetPitQuestName(QuestKind.CrazyMiller)
          
        }
      );

      pits.AddRange(PrimaryPits);
      pits.Add(DungeonPit.GetFullPitName(DungeonPit.PitGathering));

      Pits = pits.ToArray();
    }

    void Log(string log, bool error = false)
    {
      if(error)
        Container.GetInstance<ILogger>().LogError(log);
      else
        Container.GetInstance<ILogger>().LogInfo(log);
    }
       
    public override Dungeons.TileContainers.DungeonLevel Generate(int worldIndex, Dungeons.GenerationInfo info = null, Dungeons.LayouterOptions opt = null)
    {
      var gi = info as GenerationInfo;
      world = GenerateEmtyWorld(ref gi);
      this.gi = gi;

      //Add world's stuff
      Populate();

      world.EnsureRevealed(0);

      //???
      //for (int i = 0; i < 10; i++)
      //{
      //  var tile = world.GetRandomEmptyTile(DungeonNode.EmptyCheckContext.DropLoot);
      //  world.SetTile(new Wall(), tile.point);
      //}

      return world;
    }

    public World GenerateEmtyWorld(ref GenerationInfo gi)
    {
      //gi = gi as GenerationInfo;
      if (gi == null)
      {
        gi = new OuaDII.Generators.GenerationInfo();
      }
      int minWorldSize = 45;
      if (gi.MinNodeSize.Width < minWorldSize && !gi.allowSmallWordSize)
      {
        gi.MinNodeSize = new System.Drawing.Size(minWorldSize, minWorldSize);
        gi.MaxNodeSize = gi.MinNodeSize;
      }

      gi.ChildIslandAllowed = false;
      gi.NumberOfRooms = 1;//this is one big room
      gi.RevealTiles = true;
      gi.GenerateRandomInterior = false;

      var node = new World(this.Container);
      //this generates empty world (borders+empty ones)
      node.Create(width: gi.MinNodeSize.Width, height: gi.MinNodeSize.Height, info: gi, nodeIndex: 0);
      return node;
    }

    protected void Populate()
    {
      hero = gm.Container.GetInstance<Roguelike.Tiles.LivingEntities.Hero>();
      var heroInLevel = world.SetTileAtRandomPosition(hero);
      if (heroInLevel == null)
        throw new Exception("heroInLevel == null");

      hero = world.GetTiles<Roguelike.Tiles.LivingEntities.Hero>().SingleOrDefault();
      AddPits();
      hero = world.GetTiles<Roguelike.Tiles.LivingEntities.Hero>().SingleOrDefault();
      AddGroundPortals();
      hero = world.GetTiles<Roguelike.Tiles.LivingEntities.Hero>().SingleOrDefault();
      AddNPCs();
      hero = world.GetTiles<Roguelike.Tiles.LivingEntities.Hero>().SingleOrDefault();
      AddHeroChest();

      hero = world.GetTiles<Roguelike.Tiles.LivingEntities.Hero>().SingleOrDefault();
      if (gi.GenerateInteractiveTiles && GenerateDynamicTiles)
      {
        world.GenerateDynamicTiles(gm, hero, gi);
        hero = world.GetTiles<Roguelike.Tiles.LivingEntities.Hero>().SingleOrDefault();
        if (hero == null)
        {
          int k = 0;
          k++;
        }
      }
           
      world.GetTiles<Wall>().ForEach(i => i.tag1 = "Wall2");//Wall1
    }

    private void AddPits()
    {
      var pits = Pits;
           
      int pitIndex = 0;
      int prevX = 0;
      int prevY = 0;
      foreach (var pit in pits)
      {
        pitIndex++;

        int x, y;
        System.Drawing.Point pt;
        do {
          GetPitsXY(pitIndex, out x, out y);
          pt = new System.Drawing.Point(x, y);
        }
        while (world.GetTile(pt) == null || !world.GetTile(pt).IsEmpty);

        var stairs = world.AddStairsWithPit(pit, pt);
        //Log("added "+ stairs);
        Debug.Assert(stairs != null && stairs.point.X > 0 && stairs.point.Y > 0);
        Quests.QuestKind questKind = QuestManager.GetQuestKindFromPitName(pit);

        //TODO uncomment
        //if (questKind != QuestKind.Unset)
        //{
        //  var lootItems = LevelGenerator.GenerateQuestLoot(2, questKind);
        //  foreach (var lootItem in lootItems)
        //  {
        //    world.SetTile(lootItem, world.GetClosestEmpty(stairs).point);
        //  }
        //}

        prevX = x;
        prevY = y;
      }
    }

    private void GetPitsXY(int pitIndex, out int x, out int y)
    {
      var pits = Pits;
      var xStep = gi.MinNodeSize.Width / (pits.Length + 1);
      var yStep = gi.MinNodeSize.Height / (pits.Length + 1);
      x = xStep * pitIndex;
      y = yStep * pitIndex;
      int factor = RandHelper.GetRandomInt(xStep);
      bool removeFromX = RandHelper.GetRandomDouble() > 0.5f;
      if (removeFromX)
      {
        x -= factor;
        y += factor;
      }
      else
      {
        x += factor;
        y -= factor;
      }

      if (pitIndex > 1)
      {
        if (RandHelper.GetRandomDouble() > 0.33f)
        {
          x = RandHelper.GetRandomInt(world.Width - 1) + 1;
        }
      }
    }


    private void AddHeroChest()
    {
      var heroChest = new HeroChest(Container);
      world.SetTile(heroChest, world.GetEmptyTiles().First().point);
    }

    private void AddNPCs()
    {
      CreateMerchant(world, DiscussionFactory.MerchantLionelName);
      //CreateMerchant(world, DiscussionFactory.MerchantZiemowitName);
      var wanda = CreateMerchant(world, DiscussionFactory.MerchantWandaName);
      world.SetTile(wanda, world.GetEmptyTiles().Last().point);//move away from hound

      CreateNPC(world, DiscussionFactory.NPCLudoslawName);
      CreateNPC(world, DiscussionFactory.NPCNaslawName);
      CreateNPC(world, DiscussionFactory.NPCJosefName);
    }

    private void AddGroundPortals()
    {
      world.SetTile(new GroundPortal(Container) { GroundPortalKind = GroundPortalKind.Camp }, world.GetEmptyTiles().Last().point);
    }

    //private void AddLoot()
    //{
    //  var lootLevel = 1;
    //  var lg = this.Container.GetInstance<Roguelike.Generators.LootGenerator>();
    //  lg.LevelIndex = lootLevel;
    //  var loot = lg.GetRandomEquipment(Roguelike.Tiles.EquipmentKind.Weapon, 1);
    //  world.SetTile(loot, world.GetFirstEmptyPoint().Value);
    //}

    

    private OuaDII.Tiles.LivingEntities.NPC CreateNPC(World world, string npcName)
    {
      var npc = new OuaDII.Tiles.LivingEntities.NPC(Container);
      npc.Name = npcName;
      world.SetTile(npc, world.GetEmptyTiles().First().point);

      return npc;
    }

    public OuaDII.Tiles.LivingEntities.Merchant CreateMerchant(AbstractGameLevel level, string merchName)
    {
      var merchant = new OuaDII.Tiles.LivingEntities.Merchant(Container);
      merchant.Name = merchName;
      merchant.tag1 = "ally_merchant_"+ merchName;
      level.SetTile(merchant, level.GetEmptyTiles().First().point);

      var ally = Container.GetInstance<Roguelike.Tiles.LivingEntities.TrainedHound>();
      ally.tag1 = "hound";
      //ally.Kind = Roguelike.Tiles.AllyKind.Dog;
      level.SetTile(ally, level.GetClosestEmpty(merchant, true).point);

      return merchant;
    }
  }
}
