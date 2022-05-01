using Roguelike.Abstract.Projectiles;
using Roguelike.Managers;
using Roguelike.Strategy;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Policies
{
  public class ProjectileCastPolicy : Policy
  {
    LivingEntity caster;
    public GameManager GameManager { get; set; }

    public ProjectileCastPolicy()
    {
      this.Kind = PolicyKind.SpellCast;
    }

    public IProjectile Projectile { get; set; }
    public LivingEntity Caster { get => caster; set => caster = value; }
    public IProjectilesFactory ProjectilesFactory { get; set; }
    public int MaxVictimsCount 
    {
      get; 
      internal set; 
    } = 1;
    public bool ContinueAfterHit { get; set; } = false;
    //public int MaxVictimsCount = 1;
    public List<Tiles.Abstract.IObstacle> Targets = new List<Tiles.Abstract.IObstacle>();
    public ITilesAtPathProvider TilesAtPathProvider { get; set; }

    public void AddTarget(Tiles.Abstract.IObstacle obstacle)
    {
      Targets.Add(obstacle);
    }

    public void Apply(LivingEntity caster)
    {
      //if (this.Targets.Count == 1 && MaxVictimsCount > 1)
      {
        var target = this.Targets[0];
        if(MaxVictimsCount > 1)
          Targets = GetOtherVictims(caster, target);

        if(!Targets.Contains(target))
          Targets.Insert(0, target);
      }

      Apply(this.Projectile, caster, this.Targets, this.ProjectilesFactory);
    }

    public void Apply(IProjectile projectile, LivingEntity caster, List<Tiles.Abstract.IObstacle> targets, IProjectilesFactory projectilesFactory)
    {
      this.Projectile = projectile;
      this.Projectile.Count = targets.Count;
      this.caster = caster;
      Targets = targets;
      //this.Target = target as Tile;
      this.ProjectilesFactory = projectilesFactory;
      caster.State = EntityState.CastingProjectile;


      DoApply(caster);
      
      ReportApplied(caster);
    }

    //called in ascii version
    protected virtual void DoApply(LivingEntity caster)
    {
      Targets.ForEach(i=> i.OnHitBy(Projectile));
    }

    private List<Tiles.Abstract.IObstacle> GetOtherVictims(LivingEntity caster, Tiles.Abstract.IObstacle target)
    {
      var otherOnes = new List<Tiles.Abstract.IObstacle>();
      if (MaxVictimsCount > 1)
      {
        TilesAtPathProvider = GameManager.Container.GetInstance<ITilesAtPathProvider>();
        //var neibs = GameManager.CurrentNode.GetNeighborhoodTiles<Enemy>(caster, caster, 9).Distinct().ToList();//TODO 9
        if (Projectile.ActiveAbilitySrc == Abilities.AbilityKind.ArrowVolley)
        {
          List<Enemy> finalNeibs = new List<Enemy>();
          {
            var neibs = GameManager.EnemiesManager.GetInRange(caster, 7, target as Enemy);
            
            GameManager.Logger.LogInfo("GetNeighborhoodTiles<Enemy> init neibs.Count : " + neibs.Count);

            foreach (var neib in neibs)
            {
              var tiles = TilesAtPathProvider.GetTilesAtPath(caster.point, neib.point);
              if (!tiles.Any(i => i is Dungeons.Tiles.IObstacle))
              {
                finalNeibs.Add(neib);
              }
            //  //TODO check is there is no obstacle
            //  var path = GameManager.CurrentNode.FindPath(caster.point, neib.point, false, false, false, caster);
            //  if (path != null)
            //    finalNeibs.Add(neib);
            }
            //finalNeibs = neibs;
          }
          GameManager.Logger.LogInfo("GetNeighborhoodTiles<Enemy> init finalNeibs.Count : " + finalNeibs.Count);

          var en = target as Enemy;
          if (en != null)
            finalNeibs.Remove(en);
          for (int i = 1; i < MaxVictimsCount; i++)
          {
            en = finalNeibs.FirstOrDefault();
            if (en != null)
            {
              otherOnes.Add(en);
              finalNeibs.Remove(en);
            }
            else
              break;
          }
        }
        else if (Projectile.ActiveAbilitySrc == Abilities.AbilityKind.PiercingArrow)
        {
        }
      }
      return otherOnes;
    }
  }
}
