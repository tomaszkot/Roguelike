using Newtonsoft.Json.Serialization;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Serialization
{
  class SimpleInjectorContractResolver : DefaultContractResolver
  {
    private readonly Container container;

    public SimpleInjectorContractResolver(Container container)
    {
      this.container = container;
    }

    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
      // use Autofac to create types that have been registered with it
      if (container.GetRegistration(objectType) != null)
      {
        JsonObjectContract contract = ResolveContact(objectType);
        contract.DefaultCreator = () => container.GetInstance(objectType);

        return contract;
      }

      return base.CreateObjectContract(objectType);
    }

    private JsonObjectContract ResolveContact(Type objectType)
    {
      return base.CreateObjectContract(objectType);
    }
  }
}
