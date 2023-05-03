using Roguelike.Abstract.Tiles;
using Roguelike.Discussions;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Tiles.LivingEntities
{
  public class Paladin : LivingEntity, INPC, IAlly
  {
    public Paladin(Container cont) : base(Point.Empty, '!', cont)
    {
      Name = "Paladin";
    }

    public AllyBehaviour AllyBehaviour { get; set; }
    public TrainedHound TrainedHound { get ; set ; }

    public LivingEntity LivingEntity => this;

    public Discussion Discussion { get ; set ; }
    public bool HasUrgentTopic { get ; set ; }
    public bool Active { get ; set ; }

    public AllyKind Kind => AllyKind.Paladin;

    public Point Point { get => point; set => point = value; }

    public bool TakeLevelFromCaster => false;

    public void SetHasUrgentTopic(bool ut)
    {
      HasUrgentTopic = ut;
      if (UrgentTopicChanged != null)
        UrgentTopicChanged(this, HasUrgentTopic);
    }

    public bool IncreaseExp(double factor)
    {
      bool levelUp = false;
      if (levelUp  && LeveledUp !=null)
        LeveledUp(this, EventArgs.Empty);
      return true;
    }

    public void SetNextLevelExp(double exp)
    {
      
    }

    public event EventHandler<bool> UrgentTopicChanged;
    public event EventHandler LeveledUp;
  }
}
