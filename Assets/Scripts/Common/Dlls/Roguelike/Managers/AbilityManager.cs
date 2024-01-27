using Dungeons;
using Dungeons.Tiles;
using Roguelike.Abilities;
using Roguelike.Abstract.HotBar;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace Roguelike.Core.Managers
{
  public enum AbilityStartKind { Unset, WhenActivated }

  public class AbilityManager
  {
    GameManager gm;
    bool hookedToHero;
    LivingEntity activeAbilityVictim;
    public AbilityManager(GameManager gm)
    {
      this.gm = gm;

    }

    /// <summary>
    /// Called when: user assign ab to a slot 
    /// </summary>
    /// <param name="abilityUser"></param>
    /// <param name="abilityKind"></param>
    /// <param name="oldHotbarItem"></param>
    /// <returns></returns>
    public bool ActivateAbility(AdvancedLivingEntity abilityUser, AbilityKind abilityKind, IHotbarItem oldHotbarItem)
    {
      if (abilityUser is Hero && !hookedToHero)
      {
        hookedToHero = true;
        this.gm.Hero.CurrentEquipment.EquipmentChanged += CurrentEquipment_EquipmentChanged;
      }

      var ab = abilityUser.Abilities.GetAbility(abilityKind) as ActiveAbility;
      if (ab == null)
        return false;
      //var state = attacker.GetAbilityState(ab);
      //if (abilityUser.ActivatedAbilityKind != abilityKind)//state == AbilityState.Unset || oldHotbarItem == null)

      DoActivateAbility(abilityUser, ab);
      return true;
  

    }

    private void CurrentEquipment_EquipmentChanged(object sender, LootContainers.EquipmentChangedArgs e)
    {
      if (this.gm.Hero.SelectedActiveAbility != null)
      {
        DoActivateAbility(this.gm.Hero, this.gm.Hero.SelectedActiveAbility as ActiveAbility);
      }
    }


    private void DoActivateAbility(AdvancedLivingEntity attacker, ActiveAbility ab, bool fromTurnChanged = false)
    {
      
      var abilityKind = ab.Kind;

      var state = attacker.GetAbilityState(ab);
     
      var canHighlight = attacker.CanHighlightAbility(abilityKind);
      var newState = canHighlight ? AbilityState.Activated : AbilityState.Unusable;
      
      gm.AppendAction(new AbilityStateChangedEvent()
      {
        AbilityKind = abilityKind,
        AbilityState = newState,
        AbilityUser = attacker
      });

      if (newState == AbilityState.Activated)
      {
        //attacker.ActivatedAbilityKind = abilityKind;
        string reason;
        if (!attacker.CanUseAbility(ab.Kind, gm.CurrentNode, out reason))
          return;
      }

      if (newState == AbilityState.Activated && ab.RunAtActivation)
      {
        if (state == AbilityState.Working || state == AbilityState.CoolingDown)
          return;
        UseActiveAbility(abilityKind, attacker);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="targetLe"></param>
    /// <param name="turnStart"></param>
    /// <returns></returns>
    public bool UseActiveAbility(AdvancedLivingEntity attacker, bool turnStart, LivingEntity targetLe = null)
    {
      bool done = false;
      var ale = attacker;
      var ab = ale.Abilities.ActiveItems
        .Where(i => i.RunAtTurnStart == turnStart && ale.HasAbilityActivated(i))
        .SingleOrDefault();

      if (ab != null)
      {
        string reason;
        if (attacker.CanUseAbility(ab.Kind, gm.CurrentNode, out reason))
        {
          UseActiveAbility(ale.SelectedActiveAbility.Kind, ale, targetLe);
          done = true;
        }
      }

      return done;
    }

    void UseActiveAbility(AbilityKind abilityKind, AdvancedLivingEntity abilityUser, LivingEntity victim = null)
    {
      var advEnt = abilityUser as AdvancedLivingEntity;
      if (!advEnt.Abilities.IsActive(abilityKind))
        return;

      bool activeAbility = true;
      UseAbility(victim, abilityUser, abilityKind, activeAbility);
    }

    public void UseAbility(LivingEntity victim, LivingEntity abilityUser, AbilityKind abilityKind, bool activeAbility)
    {
      //abilityUser.Abili
      Ability ab = abilityUser.GetActiveAbility(abilityKind);
      if (ab is null)
        ab = abilityUser.GetPassiveAbility(abilityKind);

      bool used = false;
      if (ab is ActiveAbility aab)
        used = UseActiveAbility(aab, abilityUser as AdvancedLivingEntity, victim, sendEvent: false);
      else
      {
        if (abilityKind == AbilityKind.StrikeBack)//StrikeBack is currently only a passive one! (maybe somneday will be also active)
        {
          gm.ApplyPhysicalAttackPolicy(abilityUser, victim, (p) =>
          {
            used = true;
            abilityUser.AppendUsedAbilityAction(abilityKind);
            if (gm.AttackPolicyDone != null)
              gm.AttackPolicyDone();
          }, EntityStatKind.ChanceToStrikeBack);
        }
      }

      if (used)
      {
        if (activeAbility)
        {
          HandleActiveAbilityUsed(abilityUser, abilityKind);
        }
      }
    }

    public void HandleActiveAbilityUsed(LivingEntity abilityUser, AbilityKind abilityKind)
    {
      var ab = abilityUser.GetActiveAbility(abilityKind);
      abilityUser.HandleActiveAbilityUsed(abilityKind);
    }

    

    public Hero Hero { get => gm.Hero; }
    public AbilityStartKind AbilityStartKind { get; set; } = AbilityStartKind.WhenActivated;

    TileNeighborhood? GetTileNeighborhoodKindCompareToHero(LivingEntity target)
    {
      TileNeighborhood? neib = null;
      if (target.Position.X > Hero.Position.X)
        neib = TileNeighborhood.East;
      else if (target.Position.X < Hero.Position.X)
        neib = TileNeighborhood.West;
      else if (target.Position.Y > Hero.Position.Y)
        neib = TileNeighborhood.South;
      else if (target.Position.Y < Hero.Position.Y)
        neib = TileNeighborhood.North;
      return neib;
    }

    public bool UseActiveAbility(ActiveAbility ab, AdvancedLivingEntity ale, LivingEntity victim, bool sendEvent)
    {
      bool used = false;
      var abilityKind = ab.Kind;
      var endTurn = false;
      if (ab.CoolDownCounter > 0)
        return false;

      if (abilityKind != AbilityKind.StrikeBack)//StrikeBack is currently only a passive one!
      {
        var currentOne = ale.Abilities.ActiveItems
          .Where(i => ale.GetAbilityState(i) == AbilityState.Working)
          .SingleOrDefault();
        if (currentOne != null /*&& currentOne.Kind != ab.Kind*/)
        {
          if (currentOne.TurnsIntoLastingEffect)
          {
            var leff = ale.LastingEffectsSet.GetByAbilityKind(currentOne.Kind);
            ale.RemoveLastingEffect(leff);
          }
        }
      }

      if (abilityKind == AbilityKind.ZealAttack)
        used = true;
     
      else if (abilityKind == AbilityKind.Stride)
      {
        int horizontal = 0, vertical = 0;
        var neibKind = GetTileNeighborhoodKindCompareToHero(victim);
        if (neibKind.HasValue)
        {
          InputManager.GetMoveData(neibKind.Value, out horizontal, out vertical);
          var newPos = InputManager.GetNewPositionFromMove(victim.point, horizontal, vertical);
          activeAbilityVictim = victim;// as Enemy;
          activeAbilityVictim.MoveDueToAbilityVictim = true;

          var desc = "";
          var attack = ale.Stats.GetStat(EntityStatKind.Strength).SumValueAndPercentageFactor(ab.PrimaryStat, true);
          var damage = victim.CalcMeleeDamage(attack, ref desc);
          var inflicted = victim.InflictMeleeDamage(ale, false, ref damage, ref desc);

          gm.ApplyMovePolicy(victim, newPos.Point);
          used = true;
        }
      }
      else if (abilityKind == Abilities.AbilityKind.OpenWound)
      {
        if (victim != null)
        {
          //psk = EntityStatKind.BleedingExtraDamage;
          //ask = EntityStatKind.BleedingDuration;
          var duration = ab.AuxStat.Factor;
          var damage = Calculated.FactorCalculator.AddFactor(3, ab.PrimaryStat.Factor);//TODO  3
          victim.StartBleeding(damage, null, (int)duration);
          used = true;
        }
        else
        {
          //var leSrc = new AbilityLastingEffectSrc(ab, 0);
          //abilityUser.LastingEffectsSet.AddPercentageLastingEffect(EffectType.OpenWound, leSrc, abilityUser);
          //used = true;
        }
      }
      else if (abilityKind == AbilityKind.Smoke)
      {
        AddSmoke(ale);
        endTurn = true;
        used = true;

      }
      else
        used = AddLastingEffectFromAbility(ab, ale);

      if (used)
      {
        ale.WorkingAbilityKind = abilityKind;
        if (sendEvent)
          HandleActiveAbilityUsed(ale, abilityKind);
      }

      if (endTurn)
        gm.Context.MoveToNextTurnOwner();//TODO call HandleHeroActionDone
      return used;
    }

    LastingEffect AddAbilityLastingEffectSrc(EffectType et, ActiveAbility ab, LivingEntity abilityUser, int abilityStatIndex = 0)
    {
      var src = new AbilityLastingEffectSrc(ab, abilityStatIndex);
      if (et != EffectType.IronSkin)
      {
        if (
          ab.Kind == AbilityKind.ElementalVengeance ||
          ab.Kind == AbilityKind.Rage
          )
          src.Duration = 3;
        else
          src.Duration = 1;
      }
      else
        src.Duration = (int)ab.AuxStat.Factor;
      return abilityUser.LastingEffectsSet.AddLastingEffect(et, src, abilityUser);
    }

    public void MakeGameTick()
    {
      if (activeAbilityVictim != null)
      {
        if (activeAbilityVictim.Alive && activeAbilityVictim.State != EntityState.Idle)
          return;
        activeAbilityVictim.MoveDueToAbilityVictim = false;//AbilityKind.Stride
        activeAbilityVictim = null;
      }
    }

      public bool AddLastingEffectFromAbility(ActiveAbility ab, LivingEntity abilityUser)
    {
      if (!ab.TurnsIntoLastingEffect)
        return false;

      if (ab.CoolDownCounter > 0)
        return false;

      bool used = false;
      var abilityKind = ab.Kind;

      if (ab.Kind == AbilityKind.ElementalVengeance)
      {
        var attacks = new EffectType[] { EffectType.FireAttack, EffectType.PoisonAttack, EffectType.ColdAttack };
        int i = 0;
        foreach (var et in attacks)
        {
          AddAbilityLastingEffectSrc(et, ab, abilityUser, i);
          i++;
        }
        used = true;
      }
      else if (ab.Kind == AbilityKind.IronSkin)
      {
        AddAbilityLastingEffectSrc(EffectType.IronSkin, ab, abilityUser);
        used = true;
      }
      else if (abilityKind == AbilityKind.Rage)
      {
        AddAbilityLastingEffectSrc(EffectType.Rage, ab, abilityUser);
        used = true;
      }

      return used;
    }

    private void AddSmoke(LivingEntity abilityUser)
    {
      var smokeAb = Hero.GetActiveAbility(AbilityKind.Smoke);
      var smokes = gm.CurrentNode.AddSmoke(gm.Hero, (int)smokeAb.PrimaryStat.Factor, (int)smokeAb.AuxStat.Factor);
      gm.AppendAction<TilesAppendedEvent>((TilesAppendedEvent ac) =>
      {
        ac.Tiles = smokes.Cast<Tile>().ToList();
      });
    }

    public void HandleSmokeTiles()
    {
      var smokes = gm.CurrentNode.Layers.GetTypedLayerTiles<ProjectileFightItem>(KnownLayer.Smoke);
      ProjectileFightItem smokeEnded = null;
      foreach (var smoke in smokes)
      {
        smoke.Durability--;
        if (smoke.Durability <= 0)
        {
          gm.CurrentNode.Layers.GetLayer(KnownLayer.Smoke).Remove(smoke);
          gm.AppendAction(new LootAction(smoke, null) { Kind = LootActionKind.Destroyed, Loot = smoke });
          smokeEnded = smoke;
        }
      }
      if (smokeEnded != null && smokeEnded.ActiveAbilitySrc == AbilityKind.Smoke)
      {
        smokeEnded.Caller.StartAbilityCooling(AbilityKind.Smoke);

      }
    }

  }
}
