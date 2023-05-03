using Dungeons.Tiles.Abstract;
using OuaDII.Tiles.Looting;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Linq;

namespace OuaDII.Tiles.LivingEntities
{  
  public class God : Roguelike.Tiles.LivingEntities.God
  {
    public GodStatue GodStatue { get; set; }
    public GameManager GameManager { get; set; }
    public event EventHandler SpellCreated;

    public God(Container cont) : base(cont)
    {
      
    }

    public override bool MakeTurn()
    {
      GodStatue.PowerCoolDownCounter--;//maybe it shall happen only among enemies ?

      var ens = GameManager.EnemiesManager.GetActiveEnemies().Where(i => i.DistanceFrom(GameManager.Hero) < 8).ToList();
      if (!ens.Any())
        return true;
      
      if (GodStatue.PowerCoolDownCounter == 0)
      {
        GodStatue.PowerCoolDownCounter = GodStatue.PowerCoolDownCounterSteps;
        if (GameManager != null)
        {
          this.Point = GameManager.Hero.point;
                    
          GameManager.AppendAction(new LivingEntityAction()
          {
            Kind = LivingEntityActionKind.AppendedToLevel,
            InvolvedEntity = this,
            Info = Name + " did the turn"
          });

          //GameManager.SpellManager.ApplySpell(this, scroll);
          GameManager.Logger.LogInfo("God " + GodStatue.Name + " made the turn");
        }
        return true;
      }

      return false;
    }

    public Roguelike.Abstract.Spells.ISpell CreateSpell(ref SwiatowitScroll scroll)
    {
      scroll = new SwiatowitScroll();
      scroll.tag1 = "swiatowit_scroll";
      scroll.Kind = Scroll.DiscoverKindFromName(scroll.tag1);
      scroll.Count = 1;
      var spell = scroll.CreateSpell(this);
      if (SpellCreated != null)
        SpellCreated(this, EventArgs.Empty);
      return spell;
    }
  }
}
