using System;
using System.Diagnostics;
using System.Linq;
using Dungeons;
using Dungeons.ASCIIDisplay;
using Roguelike;
using Roguelike.Abstract;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Tiles;
using RoguelikeConsoleRunner.ASCIIDisplay;
using SimpleInjector;

namespace RoguelikeConsoleRunner
{
  public class GameController : DungeonsConsoleRunner.GameController, IGameManagerProvider
  {
    Game game;

    public GameController(Game game)
      : base(game.Container, game.LevelGenerator, game.Container.GetInstance<IDrawingEngine>())
    {
      this.game = game;
    }

    public LevelGenerator LevelGenerator { get { return game.LevelGenerator; } }
    public Container Container { get { return game.Container; } }
    public Hero Hero { get { return game.Hero; } }
    public GameManager GameManager
    {
      get { return game.GameManager; }
      set { game.GameManager = value; }//TODO remove?
    }

    protected override void Generate()
    {
      //base.Generate();
      var dungeon = game.GenerateLevel(0);
      PopulateDungeon();


      this.GameManager.EventsManager.ActionAppended += ActionsManager_ActionAppended;
      this.GameManager.Context.ContextSwitched += Context_ContextSwitched;

      var hero1 = dungeon.GetTiles<Hero>().SingleOrDefault();
      Debug.Assert(Hero == hero1);

    }

    protected virtual Roguelike.TileContainers.GameNode PopulateDungeon()
    {
      var dungeon = LevelGenerator.Dungeon;
   

      var lg = new LootGenerator();
      var loot = lg.GetRandomWeapon();
      //world.SetTile(loot, world.GetRandomEmptyTile().Point);
      dungeon.SetTile(loot, dungeon.GetFirstEmptyPoint().Value);

      //var enemy = new Enemy();
      //world.SetTile(enemy, world.GetEmptyTiles().Last().Point);
      // world.SetTile(enemy, new System.Drawing.Point(4, 1));
      return dungeon;
    }

    private void Context_ContextSwitched(object sender, EventArgs e)
    {
      Redraw();
    }

    protected override Dungeons.ASCIIDisplay.Screen CreateScreen()
    {
      screen = new ASCIIDisplay.Screen(DrawingEngine, this);
      screen.OriginX = 2;
      screen.OriginY = 2;
      //screen.DungeonY = 10;
      return screen;
    }


    private void ActionsManager_ActionAppended(object sender, Dungeons.Core.GenericEventArgs<Roguelike.GameAction> e)
    {
      if (e.EventData is LivingEntityAction)
      {
        var lea = e.EventData as LivingEntityAction;
        //if (lea.KindValue == LivingEntityAction.Kind.Moved ||
        //  lea.KindValue == LivingEntityAction.Kind.Interacted
        //  )
        {
          //
          //var snd = lea.GetSound();
          screen.Redraw(lea.InvolvedEntity, true);

          screen.RedrawLists();
          if(lea.KindValue == LivingEntityAction.Kind.Interacted)
            Redraw();//e.g. room revealed
          //DrawingEngine.SetCursorPosition(0, 0);
        }
      }
      else if (e.EventData is GameStateAction)
      {
        //var typed = e.EventData as GameStateAction;
        //if (typed.Type == GameStateAction.ActionType.EnteredLevel ||
        //  typed.Type == GameStateAction.ActionType.ContextSwitched)
        //{
        //  Redraw();
        //}
      }
      else if (e.EventData is LootAction)
      {
        screen.RedrawLists();
      }
    }


    public override DungeonNode Dungeon
    {
      get
      {
        return GameManager.Context.CurrentNode;
      }
    }
    
    protected override bool HandleKey(ConsoleKeyInfo info)
    {
      int vertical = 0;
      int horizontal = 0;
      var key = info.Key;

      if (key == ConsoleKey.G)
      {
        if (GameManager.CollectLootOnHeroPosition())
        {

        }
      }
      else if (key == ConsoleKey.S)
      {
        GameManager.Save();
      }
      else if (key == ConsoleKey.L)
      {
        GameManager.Load();
      }
      else if (key == ConsoleKey.Spacebar)
      {
        GameManager.DoAlliesTurn(true);
      }
      else if (key == ConsoleKey.LeftArrow)
      {
        horizontal = -1;
      }
      else if (key == ConsoleKey.RightArrow)
      {
        horizontal = 1;
      }
      else if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
      {
        vertical = -1;
      }
      else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
      {
        vertical = 1;
      }

      if (horizontal != 0 || vertical != 0)
      {
        if ((info.Modifiers & ConsoleModifiers.Control) != 0)
        {
          //gm.UseHeroSpellAtDirection(horizontal, vertical, Point.Invalid);
        }
        else
        {
          GameManager.HandleHeroShift(horizontal, vertical);
        }
      }

      if ((info.Modifiers & ConsoleModifiers.Control) != 0)
      {
        if (key == ConsoleKey.N)
        {
          //next level
        }
      }
      return base.HandleKey(info);

    }

    protected override void Redraw()
    {
      if (GameManager == null)
        return;
      //Debug.Assert(Dungeon == DungeonPresenter.Node);
      //Debug.Assert(gameManager.Hero == Dungeon.GetTiles<Hero>().SingleOrDefault()); 
      base.Redraw();
      
    }
  }
}
