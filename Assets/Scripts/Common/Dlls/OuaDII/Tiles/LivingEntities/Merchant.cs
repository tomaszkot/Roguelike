using OuaDII.Discussions;
using SimpleInjector;

namespace OuaDII.Tiles.LivingEntities
{
  public class Merchant : Roguelike.Tiles.LivingEntities.Merchant
  {
    //TrainedHound trainedHound;

    public Merchant(Container cont) : base(cont)
    {
    }
        
    public Discussion OuaDDiscussion
    {
      get { return base.Discussion as Discussion; }
    }
  }
}
