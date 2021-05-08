using Dungeons.Core;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.LivingEntities
{
  public class NPC : AdvancedLivingEntity
  {
    public NPC(Container cont) : base(cont, new Point().Invalid(), '!')
    {
    }
  }
}
