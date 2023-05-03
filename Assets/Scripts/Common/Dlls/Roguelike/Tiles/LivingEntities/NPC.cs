using Dungeons.Core;
using Roguelike.Discussions;
using Roguelike.Tiles.Interactive;
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

  public class NPC : AdvancedLivingEntity, INPC, IApproachableByHero
  {
    public TrainedHound TrainedHound { get; set; }

    public NPC(Container cont) : base(cont, new Point().Invalid(), '!')
    {
    }

    public LivingEntity LivingEntity => this;

    public bool ApproachedByHero { get ; set ; }
    public string ActivationSound { get; set; }

    public event EventHandler Activated;

    public bool Activate()
    {
      if (!ApproachedByHero)
      {
        ApproachedByHero = true;
        if (Activated != null)
          Activated(this, EventArgs.Empty);
        return true;
      }
      return false;
    }

    public string GetPlaceName()
    {
      return "";
    }
  }
}
