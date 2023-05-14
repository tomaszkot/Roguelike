using Dungeons.ASCIIDisplay;
using Godot;
using Roguelike.Abstract.Multimedia;
using Roguelike.Abstract;
using Roguelike;
using Roguelike.Managers;
using Roguelike.Multimedia;
using System;
using Roguelike.Events;
using System.IO;
using Dungeons.Tiles;

public partial class Game : Node2D
{
  GameManager gm;
  IGame game;

  public GameManager GameManager
  {
    get { return game.GameManager; }
    set { game.GameManager = value; }//TODO remove?
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
	{
    //gm.LoadLevel("Hero1", 0, false);

    var container = new Roguelike.ContainerConfigurator().Container;

    //container.Register<GameController, GameController>();
    container.Register<IDrawingEngine, ConsoleDrawingEngine>();
    container.Register<IGame, RoguelikeGame>();
    container.Register<ISoundPlayer, BasicSoundPlayer>();

    game = container.GetInstance<IGame>();
    this.GameManager.EventsManager.EventAppended += ActionsManager_ActionAppended;
    this.GameManager.Context.ContextSwitched += Context_ContextSwitched;

    GD.Print("GenerateDungeon...");
    game.GenerateDungeon();

    var scene = GD.Load<PackedScene>("res://hero.tscn");
    var instance = scene.Instantiate();
    AddChild(instance);
  }

  private void Context_ContextSwitched(object sender, ContextSwitch e)
  {
    if (e.Kind == GameContextSwitchKind.NewGame)
    {
      GameManager.Hero.Name = "Godot Hero";
      var tiles = GameManager.CurrentNode.Tiles;

      foreach (var tile in tiles)
      {
        if (tile is Wall)
        {
          var scene = GD.Load<PackedScene>("res://Entities/Wall.tscn");
          var instance = scene.Instantiate();
          var spr = instance.GetChild<Godot.Sprite2D>(0);
          spr.GlobalPosition = new Vector2(tile.point.X* Sprite2D.TileSize, tile.point.Y* Sprite2D.TileSize);
          AddChild(instance);
        }
      }
    }
  }

  private void ActionsManager_ActionAppended(object sender, GameEvent e)
  {
    if (e is LivingEntityAction)
    {
      var lea = e as LivingEntityAction;
      //screen.Redraw(lea.InvolvedEntity, true);
    }
    else if (e is GameStateAction)
    {
    }
    else if (e is LootAction)
    {
    }
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
	{
    game.MakeGameTick();
  }
}
