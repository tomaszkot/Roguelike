using Roguelike.Attributes;
using Roguelike.Spells;
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
      TakeLevelFromCaster = true;
      Kind = AllyKind.Hound;
      Name = "Hound";
#if ASCII_BUILD
        color = ConsoleColor.Yellow;
#endif
    }

    public override float GetStartStat(EntityStatKind esk)
    {
      var startStat = base.GetStartStat(esk);
      if (esk == EntityStatKind.Strength)
        startStat += SkeletonSpell.SkeletonSpellStrengthIncrease;

      else if (esk == EntityStatKind.Defense)
        startStat += SkeletonSpell.SkeletonSpellDefenseIncrease;

      else if (esk == EntityStatKind.Health)
        startStat += SkeletonSpell.SkeletonSpellHealthIncrease;

      return startStat;
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
