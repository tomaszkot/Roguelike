using Dungeons.Core;
using Roguelike.Discussions;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.LivingEntities
{
  public interface INPC
  {
    Discussion Discussion { get; set; }
    void SetHasUrgentTopic(bool ut);
    string Name { get; }
    TrainedHound TrainedHound { get; set; }
    RelationToHero RelationToHero { get; set; }
    AdvancedLivingEntity AdvancedLivingEntity { get;}
  }

  public class NPC : AdvancedLivingEntity, INPC
  {
    public TrainedHound TrainedHound { get; set; }

    public NPC(Container cont) : base(cont, new Point().Invalid(), '!')
    {
    }

    public AdvancedLivingEntity AdvancedLivingEntity => this;
  }
}
