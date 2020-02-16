using Dungeons.Core;
using Roguelike.Events;
using Roguelike.InfoScreens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Managers
{
  public class EventsManager
  {
    LastActions lastActions = new LastActions();

    public LastActions LastActions { get => lastActions; set => lastActions = value; }

    public event EventHandler<GenericEventArgs<GameAction>> ActionAppended;

    public void AppendAction(GameAction ac)
    {
      LastActions.Add(ac);
      // //Debug.WriteLine(ac);//slow
      if (ActionAppended != null)//..but send it to listeners as logic of game depends on it
        ActionAppended(this, new GenericEventArgs<GameAction>(ac));
    }
  }
}
