using Dungeons.Core;
using Roguelike.Discussions;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.LivingEntities
{
  public interface INPC
  {
    //string Name { get; }
    TrainedHound TrainedHound { get; set; }
    AdvancedLivingEntity AdvancedLivingEntity { get;}
    Discussion Discussion { get; set; } 
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
