using Dungeons.ASCIIDisplay.Presenters;
using Dungeons.Tiles;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.InfoScreens
{
  public class LastActions
  {
    private List<GameAction> actions = new List<GameAction>();
    const int MaxSize = 5;
    bool reverse = false;
    int actionsCount = 0;

    public LastActions()
    {

      //Func<HeroAction, bool?> acHero = (HeroAction x) =>
      //{
      //  if (x.KindValue == HeroAction.Kind.Moved)
      //    return false;
      //  return true;
      //};
      //Func<LootAction, bool?> acLoot = (LootAction x) =>
      //{
      //  if (x.KindValue == LootAction.Kind.Generated)
      //    return false;
      //  return true;
      //};
      //Func<EnemyAction, bool?> acEnemy = (EnemyAction x) =>
      //{
      //  if (x.KindValue == EnemyAction.Kind.Moved)
      //    return false;
      //  if (x.KindValue == EnemyAction.Kind.ChasingPlayer)
      //    return false;
      //  if (x.KindValue == EnemyAction.Kind.AppendedToLevel)
      //    return false;
      //  return true;
      //};
      //actionsSwitch = new TypeSwitch<GameAction, bool?>()
      // .Case(acHero)
      //  .Case(acEnemy)
      //  .Case(acLoot)
      // ;

      for (int i = 0; i < MaxSize; i++)
      {
        Add("");
      }
    }

    public List<GameAction> Actions
    {
      get
      {
        return actions;
      }
    }

    internal void Add(GameAction ac)
    {
      if (ac.Info.Trim() == string.Empty)
        return;

      //if (actionsSwitch.ContainsSwitch(ac.GetType()))
      //{
      //  var display = actionsSwitch.Switch(ac);
      //  if (display.HasValue && display.Value == false)
      //    return;
      //}

      if (reverse)
        actions.Insert(0, ac);
      else
        actions.Add(ac);
      ac.Index = ++actionsCount;
      //ac.Info = actionsCount.ToString() + ") " + ac.Info;
      if (actions.Count > MaxSize)
      {
        if (reverse)
          actions.RemoveAt(actions.Count - 1);
        else
          actions.RemoveAt(0);
      }
    }


    internal void Add(string action, ActionLevel level = ActionLevel.Normal)
    {
      var ac = new GameAction() { Info = action, Level = level };
      Add(ac);
    }

    public List<ListItem> ToASCIIList()
    {
      var lines = new List<ListItem>();
      foreach (var ac in Actions)
      {
        lines.Add(new ListItem()
        {
          Color = ac.Level == ActionLevel.Normal ? ConsoleColor.White : ConsoleColor.DarkMagenta,
          Text = ac.Info
        });
      }

      return lines;
    }
  }



}
