using Dungeons;
using Dungeons.ASCIIDisplay;
using Roguelike;
using Roguelike.Abstract;
using Roguelike.Abstract.Managers;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Linq;

namespace RoguelikeConsoleRunner
{
  public class GameController : DungeonsConsoleRunner.GameController, IGameManagerProvider
  {
    IGame game;
    IDungeonGenerator generator;
    public IGame Game { get => game; set => game = value; }

    public GameController(IGame game, IDungeonGenerator generator)
      : base(game.Container, generator, game.Container.GetInstance<IDrawingEngine>())
    {
      //game.Container.Register<ISoundPlayer, AppSoundPlayer>();
      this.Game = game;
      this.generator = generator;

      this.GameManager.EventsManager.ActionAppended += ActionsManager_ActionAppended;
      this.GameManager.Context.ContextSwitched += Context_ContextSwitched;
    }

    protected override void HandleNextGameTick()
    {
      base.HandleNextGameTick();
      Game.MakeGameTick();
    }

    //[JsonIgnore]
    public Container Container { get { return Game.Container; } }
    public Hero Hero { get { return Game.Hero; } }
    public GameManager GameManager
    {
      get { return Game.GameManager; }
      set { Game.GameManager = value; }//TODO remove?
    }

    public override Dungeons.TileContainers.DungeonNode GenerateDungeon()
    {
      var dungeon = Game.GenerateDungeon();

      //var hero1 = dungeon.GetTiles<Hero>().SingleOrDefault();
      //Debug.Assert(Hero == hero1);
      return dungeon;
    }

    protected virtual void PopulateDungeon(Roguelike.TileContainers.AbstractGameLevel dungeon)
    {

    }

    private void Context_ContextSwitched(object sender, ContextSwitch context)
    {
      if (context.Kind == GameContextSwitchKind.NewGame)
        GameManager.Hero.Name = "ConsoleHero";
      Redraw();
    }

    protected override Dungeons.ASCIIDisplay.Screen CreateScreen()
    {
      screen = new ASCIIDisplay.Screen(DrawingEngine, this);
      screen.OriginX = 2;
      screen.OriginY = 2;
      return screen;
    }


    private void ActionsManager_ActionAppended(object sender, GameEvent e)
    {
      if (e is LivingEntityAction)
      {
        var lea = e as LivingEntityAction;
        screen.Redraw(lea.InvolvedEntity, true);

        screen.RedrawLists();
        if (lea.Kind == LivingEntityActionKind.Interacted && lea.InteractionResult != InteractionResult.Attacked)
          Redraw();//e.g. room revealed
      }
      else if (e is GameStateAction)
      {
      }
      else if (e is LootAction)
      {
        screen.RedrawLists();
        var la = e as LootAction;
        screen.Redraw(la.Loot, false);
      }
    }

    public override Dungeons.TileContainers.DungeonNode Dungeon
    {
      get
      {
        return GameManager.Context.CurrentNode;
      }
    }

    //string HeroName = "";

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
      //else if (key == ConsoleKey.S)
      //{
      //  GameManager.Save();
      //  HeroName = GameManager.Hero.Name;
      //}
      else if (key == ConsoleKey.L)
      {
        if (GameManager.Hero.Name.Any())
        {
          GameManager.Load(GameManager.Hero.Name);
          Redraw();
        }
      }
      else if (key == ConsoleKey.Spacebar)
      {
        //GameManager.DoAlliesTurn(true);
        GameManager.SkipHeroTurn();
      }
      else if (key == ConsoleKey.LeftArrow || key == ConsoleKey.A)
      {
        horizontal = -1;
      }
      else if (key == ConsoleKey.RightArrow || key == ConsoleKey.D)
      {
        horizontal = 1;
      }
      else if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
      {
        vertical = -1;
      }
      else if (key == ConsoleKey.DownArrow)
      {
        vertical = 1;
      }

      else if (key == ConsoleKey.S)
      {
        GameManager.Save();
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

      //if ((info.Modifiers & ConsoleModifiers.Control) != 0)
      {
        if (key == ConsoleKey.N)
        {
          //next level
          HandleNextLevelDown();
        }
      }
      return base.HandleKey(info);

    }

    protected virtual bool HandleNextLevelDown()
    {
      var down = GameManager.CurrentNode.GetStairs(StairsKind.LevelDown);
      if (down != null)
      {
        GameManager.InteractHeroWith(down);
        return true;
      }

      return false;
    }

    protected override void Redraw()
    {
      if (GameManager == null)
        return;
      //Debug.Assert(Dungeon == DungeonPresenter.Node);
      //Debug.Assert(gameManager.Hero == Dungeon.GetTiles<Hero>().SingleOrDefault()); 
      base.Redraw();
    }

    protected override void RevealAll()
    {
      base.RevealAll();
      var node = GameManager.CurrentNode;
      node.Reveal(true, true);
      Redraw();
    }
  }
}
