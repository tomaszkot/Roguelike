using Dungeons;
using Dungeons.ASCIIDisplay;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      while (!exit)
      {
        var key = Console.ReadKey(true);
        exit = HandleKey(key);
      }
    }

    protected virtual void GenerateDungeon()
    {
      Dungeon = generator.Generate(levelIndex++);
      
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

      return exit;
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
