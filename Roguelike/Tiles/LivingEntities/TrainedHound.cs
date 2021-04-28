using SimpleInjector;

namespace Roguelike.Tiles.LivingEntities
{
  public class TrainedHound : Ally
  {
    //public TrainedHound() : base(null)
    //{
    //  throw new Exception("use other ctor");
    //}

    public TrainedHound(Container cont) : base(cont)
    {
      Kind = AllyKind.Hound;
      Name = "Hound";
#if ASCII_BUILD
        color = ConsoleColor.Yellow;
#endif
    }

    public void bark(bool strong)
    {
      PlaySound("ANIMAL_Dog_Bark_02_Mono");
    }

    public override void PlayAllySpawnedSound()
    {
      bark(false);
    }

    public override void SetTag()
    {
      tag1 = "hound";
    }
  }
}
