using SimpleInjector.Advanced;
using System;
using System.Linq;
using System.Reflection;

namespace Dungeons.Core
{
  public class GreediestConstructorBehavior : IConstructorResolutionBehavior
  {
    public ConstructorInfo GetConstructor(Type implementationType) => (
        from ctor in implementationType.GetConstructors()
        orderby ctor.GetParameters().Length //descending
        select ctor)
        .First();
  }
}
