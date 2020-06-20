using Dungeons;
using Dungeons.ASCIIDisplay;
using Dungeons.TileContainers;
using SimpleInjector;
using System;

namespace DungeonsConsoleRunner
{
  public class GameController
  {
    IDungeonGenerator generator;

    public virtual DungeonNode Dungeon { get; set; }
    public IDrawingEngine DrawingEngine { get; set; }
    protected Screen screen;
    Container container;
    int levelIndex;

    public GameController(Container container, IDungeonGenerator generator, IDrawingEngine drawingEngine)
    {
      this.container = container;
      this.generator = generator;
      this.DrawingEngine = drawingEngine;
    }

    protected virtual Screen CreateScreen()
    {
      screen = container.GetInstance<Screen>();
      return screen;
    }

    public void Run()
    {
      ConsoleSetup.Init();
      GenerateDungeon();
      Redraw();

      bool exit = false;
      var buf = new byte[2048];
      while (!exit)
      {
        if (Console.KeyAvailable)
        {
          var key = Console.ReadKey(true);
          exit = HandleKey(key);
        }
        if (!exit)
        {
          HandleNextGameTick();
        }
      }
    }

    protected virtual void HandleNextGameTick()
    {
    }

    public virtual DungeonNode GenerateDungeon()
    {
      Dungeon = generator.Generate( levelIndex++);
      return Dungeon;
    }

    protected virtual bool HandleKey(ConsoleKeyInfo key)
    {
      bool exit = false;
      var input = key.Key;
      if (input == ConsoleKey.Escape)
        exit = true;
      if (input == ConsoleKey.R)
        Reload();
      if (input == ConsoleKey.D)
      {
        screen.PrintInfo.PrintNodeIndexes = !screen.PrintInfo.PrintNodeIndexes;
        Redraw();
      }
      if (input == ConsoleKey.V)
      {
        RevealAll();
      }

      return exit;
    }

    protected virtual void RevealAll()
    {
      
    }

    protected void Reload()
    {
      GenerateDungeon();
      Redraw();
    }

    protected virtual void Redraw()
    {
      if (screen == null)
      {
        screen = CreateScreen();
      }
      screen.Redraw(Dungeon);
    }
  }
}
