using Dungeons;
using Roguelike.Events;
using Roguelike.InfoScreens;
using System;
using System.Diagnostics;

namespace Roguelike.Managers
{
  public class EventsManager
  {
    LastActions lastActions = new LastActions();

    public LastActions LastActions { get => lastActions; set => lastActions = value; }
    public GameManager GameManager { get => gameManager; set => gameManager = value; }

    public event EventHandler<GameEvent> EventAppended;
    GameManager gameManager;

    public EventsManager()
    {

    }

    public void Assert(bool check, string desc)
    {
      if (!check)
      {
        AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.Assert, Info = desc });
        DebugHelper.Assert(false);
      }
    }

    public const string TurnHint1 = "/";
    public const string TurnHint2 = "\\";

    public static string CurrentTurnHint = TurnHint1;

    public static void GotoNextTurnHint()
    {
      if (CurrentTurnHint == TurnHint1)
        CurrentTurnHint = TurnHint2;
      else
        CurrentTurnHint = TurnHint1;
    }

    public void AppendAction(GameEvent ac)
    {
      if (GameManager != null)//Main Menu?
      {
        if (GameManager.Hero != null && !GameManager.Hero.Alive && GameManager.Context.HeroDeadReported)
          return;
      }

      if(LastActions.Contains(ac))
        return;

      ac.TurnHint = CurrentTurnHint;
      LastActions.Add(ac);
      if (EventAppended != null)//send it to listeners as logic of game depends on it
      {
        try
        {
          EventAppended(this, ac);
        }
        catch (Exception ex)
        {
          gameManager.Logger.LogError(ex);
          //that exc can not be propagated as it would cause severe errors (like loot disappearing)
          //throw;

          //Caused recursive call to AppendAction
          //Assert(false, ex.Message + "\r\n"+ ex.StackTrace);


          //beneath code added debug string to the game window!
          //if (!LastActions.Contains(ac))
            //LastActions.Add(ex.Message + "\r\n" + ex.StackTrace);
        }
      }
    }
  }
}
