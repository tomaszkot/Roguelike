using Dungeons.Core;
using Roguelike.Discussions;
using SimpleInjector;
using System;
using System.Drawing;

namespace Roguelike.Tiles.LivingEntities
{
  public interface INPC
  {
    string Name { get; }
    TrainedHound TrainedHound { get; set; }
    LivingEntity LivingEntity { get;}
    Discussion Discussion { get; set; }
    void SetHasUrgentTopic(bool ut);
    bool HasUrgentTopic { get; set; }
    event EventHandler<bool> UrgentTopicChanged;
  }

  public class NPC : AdvancedLivingEntity, INPC
  {
    public TrainedHound TrainedHound { get; set; }

    public NPC(Container cont) : base(cont, new Point().Invalid(), '!')
    {
    }

    public LivingEntity LivingEntity => this;
  }
}
