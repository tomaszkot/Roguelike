using SimpleInjector.Advanced;
using System;
using System.Linq;
using System.Reflection;
#pragma warning disable 8603
#pragma warning disable 8602

namespace Dungeons.Core
{
  public class GreediestConstructorBehavior : IConstructorResolutionBehavior
  {
    public ConstructorInfo GetConstructor(Type implementationType)
    {
      string errorMessage;
      return TryGetConstructor(implementationType, out errorMessage);
    }
#nullable enable
    public ConstructorInfo? TryGetConstructor(Type implementationType, out string? errorMessage)
    {
      errorMessage = "";
      var res = (from ctor in implementationType.GetConstructors()
                 orderby ctor.GetParameters().Length //descending
                 select ctor)
        .FirstOrDefault();
      if (res == null)
        errorMessage = "res == null!";
      return res;
    }

#nullable disable
  }
}
