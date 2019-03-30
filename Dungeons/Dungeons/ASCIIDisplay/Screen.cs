﻿using Dungeons.ASCIIDisplay.Presenters;
using Dungeons.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons.ASCIIDisplay
{
  public class Screen
  {
    protected const string UsageListName = "Usage";
    PrintInfo printInfo = new PrintInfo();
    public int OriginX { get; set; }
    public int OriginY { get; set; }
    public int DungeonX { get; set; }
    public int DungeonY { get; set; }

    public DungeonPresenter DungeonPresenter { get; set; }
    public PrintInfo PrintInfo { get => printInfo; set => printInfo = value; }
    public Dictionary<string, ListPresenter> Lists { get => lists; set => lists = value; }
    public List<Item> ASCIIItems = new List<Item>();

    Dictionary<string, ListPresenter> lists;
    protected IDrawingEngine DrawingEngine;
    DungeonNode dungeon;
    public virtual DungeonNode Dungeon { get { return dungeon; } }
    public bool UpdateUI { get; set; } = true;
    public Label DungeonDesc { get => dungeonDesc; set => dungeonDesc = value; }

    Label dungeonDesc;

    public Screen(IDrawingEngine drawingEngine)
    {
      this.DrawingEngine = drawingEngine;
      
    }

    public void Redraw(Tile tile, bool alsoNeibs)
    {
      DungeonPresenter.RefreshPosition(Dungeon, null, tile.Point.X, tile.Point.Y);
      if(alsoNeibs)
        RefreshNeibs(tile);
    }

    private void RefreshNeibs(Tile tile)
    {
      var neibs = Dungeon.GetNeighborTiles(tile, true).Where(i=> i!= null).ToList();
      RedrawTiles(neibs);
    }

    private void RedrawTiles<T>(List<T> tiles) where T : Tile
    {
      tiles.ForEach(i=> Redraw(i, false));
    }
    
    protected virtual void CreateLists()
    {
      Lists = new Dictionary<string, ListPresenter>();
      var usage = new ListPresenter(UsageListName, OriginX, OriginY, 30);
      var list = new List<ListItem>();
      list.Add(new ListItem("R - reload"));
      list.Add(new ListItem("D - toggle node_indexes/symbols"));
      list.Add(new ListItem("Esc - exit"));
      usage.Items = list;
      Lists[UsageListName] = usage;
    }

    public virtual void CreateUI()
    {
      CreateLists();
      DungeonY = Lists[UsageListName].TotalHeight;// 1 - Dungeon.Description, 2 - spacing

      DungeonDesc = new Label(OriginX, OriginY + Lists[UsageListName].TotalHeight, Dungeon.Description);
      ASCIIItems.Add(DungeonDesc);

      DungeonY += DungeonDesc.TotalHeight;
      DungeonY += 2;

    }

    public virtual void Redraw(DungeonNode dungeon)
    {
      this.dungeon = dungeon;

      if (!ASCIIItems.Any())
      {
        CreateUI();
      }

      DrawingEngine.Clear();
      if (UpdateUI)
      {
        RedrawLists();

        RedrawItems();
      }

      if (DungeonPresenter == null)
      {
        DungeonPresenter = new DungeonPresenter(DrawingEngine, OriginX + DungeonX, OriginY + DungeonY);
      }
      //Debug.Assert(dungeon == Dungeon);
      DungeonPresenter.Redraw(dungeon, PrintInfo);
      DrawingEngine.SetCursorPosition(0, 0);
    }

    private void RedrawItems()
    {
      
      ASCIIItems.ForEach(i =>
      {
        UpdateItem(i);
        i.Redraw(DrawingEngine);
        

      }
      );
    }

    protected virtual void UpdateItem(Item i)
    {
      
    }

    public virtual void RedrawLists()
    {
      if (!UpdateUI)
        return;
      foreach (var list in Lists)
      {
        UpdateList(list.Value);
        list.Value.Redraw(DrawingEngine);
      }
    }

    public virtual void UpdateList(ListPresenter list)
    {
    }
  }
}
