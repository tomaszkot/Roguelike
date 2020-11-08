﻿using Roguelike.Abstract;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Managers
{
  public class AlliesManager : EntitiesManager
  {
    public AlliesManager(GameContext context, EventsManager eventsManager, Container container) :
                         base(TurnOwner.Allies, context, eventsManager, container)
    {
      context.TurnOwnerChanged += OnTurnOwnerChanged;
      context.ContextSwitched += Context_ContextSwitched;
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      UpdateEntities();
    }

    private void UpdateEntities()
    {
      var allies = Context.CurrentNode.GetTiles<LivingEntity>().Where(i => i is Abstract.IAlly).Cast<IAlly>();
      var entities = allies.Where(i=> i.Active).Cast<LivingEntity>().ToList();
      base.SetEntities(entities);
    }

    protected override void OnPolicyAppliedAllIdle()
    {
      if (context.TurnOwner == TurnOwner.Allies)//for ASCII/UT
      {
        context.IncreaseActions(TurnOwner.Allies);
        base.OnPolicyAppliedAllIdle();
      }
    }

    private void OnTurnOwnerChanged(object sender, TurnOwner turnOwner)
    {
    }
  }
}