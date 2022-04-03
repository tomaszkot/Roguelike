using Roguelike.Abstract.Projectiles;
using Roguelike.Managers;
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

        Targets.Insert(0, target);
      }

      Apply(this.Projectile, caster, this.Targets, this.ProjectilesFactory);
    }

    public void Apply(IProjectile projectile, LivingEntity caster, List<Tiles.Abstract.IObstacle> targets, IProjectilesFactory projectilesFactory)
    {
      this.Projectile = projectile;
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
        //var neibs = GameManager.CurrentNode.GetNeighborhoodTiles<Enemy>(caster, caster, 9).Distinct().ToList();//TODO 9
        if (Projectile.ActiveAbilityKind == Abilities.AbilityKind.ArrowVolley)
        {
          var neibs = GameManager.EnemiesManager.GetInRange(caster, 7, target as Enemy);
          GameManager.Logger.LogInfo("GetNeighborhoodTiles<Enemy> neibs.Count : " + neibs.Count);
          var en = target as Enemy;
          if (en != null)
            neibs.Remove(en);
          for (int i = 1; i < MaxVictimsCount; i++)
          {
            en = neibs.FirstOrDefault();
            if (en != null)
            {
              otherOnes.Add(en);
              neibs.Remove(en);
            }
            else
              break;
          }
        }
        else if (Projectile.ActiveAbilityKind == Abilities.AbilityKind.PiercingArrow)
        {
        }
      }
      return otherOnes;
    }
  }
}
