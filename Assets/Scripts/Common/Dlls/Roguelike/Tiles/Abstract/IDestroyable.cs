using Newtonsoft.Json;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;

namespace Roguelike.Tiles.Abstract
{
  

  public interface IDestroyable : ILootSource, IObstacle
  {
    bool Destroyed { get; set; }

   // void PlayHitSound(ProjectileFightItem pfi);
    //void AppendAction(GameEvent ac);

  }
}
