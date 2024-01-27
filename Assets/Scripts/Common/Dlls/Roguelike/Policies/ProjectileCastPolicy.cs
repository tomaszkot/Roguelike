using Dungeons.Tiles.Abstract;
using Roguelike.Abstract.Projectiles;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Managers;
using Roguelike.Strategy;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Policies
{
  public class ProjectileCastPolicy : AttackPolicy
  {
    LivingEntity caster;
    public GameManager GameManager { get; set; }

    public ProjectileCastPolicy() : this(null)
    {

    }

    public ProjectileCastPolicy(Container container)
    {
      this.Kind = PolicyKind.SpellCast;
      if (container != null)
        Init(container);
    }

    public void Init(Container container)
    {
      ProjectilesFactory = container.GetInstance<IProjectilesFactory>();
    }

    public Abstract.Projectiles.IProjectile Projectile { get; set; }
    public LivingEntity Caster { get => caster; set => caster = value; }
    public IProjectilesFactory ProjectilesFactory { get; set; }
    public int MaxVictimsCount 
    {
      get; 
      internal set; 
    } = 1;
    public bool ContinueAfterHit { get; set; } = false;

    public ITilesAtPathProvider TilesAtPathProvider { get; set; }

    public override void Apply(LivingEntity caster)
    {
        var target = this.Targets[0];
        if(MaxVictimsCount > 1)
          Targets = GetOtherVictims(caster, target);

        if(!Targets.Contains(target))
          Targets.Insert(0, target);

      Apply(this.Projectile, caster, this.Targets, this.ProjectilesFactory);
    }

    public void Apply(Abstract.Projectiles.IProjectile projectile,
      LivingEntity caster, 
      List<Dungeons.Tiles.Abstract.IHitable> targets, 
      IProjectilesFactory projectilesFactory)
    {
      this.Projectile = projectile;
      this.Projectile.Count = targets.Count;
      this.caster = caster;
      Targets = targets;

      //this.Target = target as Tile;
      this.ProjectilesFactory = projectilesFactory;
      caster.State = EntityState.CastingProjectile;

      //Log("calling  DoApply(caster)");
      DoApply(caster);
      
      ReportApplied(caster);
    }

    //called in ascii version
    protected virtual void DoApply(LivingEntity caster)
    {
      Targets.ForEach(i=> AttackNextTarget(caster, i));
    }

    private List<Dungeons.Tiles.Abstract.IHitable> GetOtherVictims(LivingEntity caster, Dungeons.Tiles.Abstract.IHitable target)
    {
      var otherOnes = new List<IHitable>();
      bool cannon = false;
      if (Projectile is ProjectileFightItem pfi && pfi.FightItemKind == FightItemKind.CannonBall)
      {
        cannon = true;
      }
      if (MaxVictimsCount > 1)
      {
        TilesAtPathProvider = GameManager.Container.GetInstance<ITilesAtPathProvider>();
        if (Projectile.ActiveAbilitySrc == Abilities.AbilityKind.ArrowVolley)
        {
          List<Enemy> finalNeibs = new List<Enemy>();
          {
            var neibs = GameManager.EnemiesManager.GetInRange(caster, 7, target as Enemy);
            
            GameManager.Logger.LogInfo("GetNeighborhoodTiles<Enemy> init neibs.Count : " + neibs.Count);

            foreach (var neib in neibs)
            {
              var tiles = TilesAtPathProvider.GetTilesAtPath(caster.point, neib.point);
              if (!tiles.Any(i => i is IObstacle))
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
        else if (cannon)
        {
          var pfiC = Projectile as ProjectileFightItem;
          var ab = caster.GetActiveAbility(FightItem.GetAbilityKind(pfiC));
          string res = "";

          var neibs = GameManager.EnemiesManager.GetInRange(caster, 8, target as Enemy);
          
          var neib = neibs.Where(i => i != target &&
          (
            i.point.Y == caster.Position.Y
            ||
            i.point.X == caster.Position.X
          ))
          .Where(i=>caster.CanUseAbility(ab.Kind, GameManager.CurrentNode, out res, i))
          .ToList();
          
          otherOnes.AddRange(neib);
        }
      }
      return otherOnes;
    }

    public override void AttackNextTarget
    (
      LivingEntity caster,
      IHitable nextTarget
    )
    {
      var ak = AttackKind.PhysicalProjectile;
      if (Projectile is IProjectileSpell ps)
        ak = spellSource.IsManaPowered ? AttackKind.SpellElementalProjectile : AttackKind.WeaponElementalProjectile;
      TryAttack(this, caster, nextTarget, ak, Projectile);

    }

    //do not delete! used by UI
    public static bool ShallHitTarget(
      Dungeons.Tiles.Abstract.IProjectile projectile, 
      Dungeons.Tiles.Abstract.IHitable nextTarget, 
      LivingEntity caster
      )
    {
      if (nextTarget is LivingEntity le && projectile is ProjectileFightItem pfi)
      {
        if (!caster.CalculateIfHitWillHappen(le, AttackKind.PhysicalProjectile, pfi))
        {
          projectile.MissedTarget = true;
          caster.EventsManager.AppendAction(new LivingEntityAction(LivingEntityActionKind.Missed)
          {
            InvolvedEntity = caster,
            Missed = nextTarget,
            Info = pfi.FightItemKind.ToDescription() + " from " + caster.Name + " missed " + le.Name

          });
          return false;
        }
      }
      return true;
    }

    SpellSource spellSource;
    public override Dungeons.Tiles.Abstract.ISpell CreateSpell(LivingEntity caster, SpellSource spellSource)
    {
      this.spellSource = spellSource;
      var spell = spellSource.CreateSpell(caster) as IProjectileSpell;
      Projectile = spell;
      return spell;
    }

  }
}
