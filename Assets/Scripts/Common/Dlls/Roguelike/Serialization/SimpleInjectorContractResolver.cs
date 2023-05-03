using Newtonsoft.Json.Serialization;
using SimpleInjector;
using System;
using System.Diagnostics;

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
        //Debug.WriteLine(objectType.FullName+ " GetRegistration - true");
        JsonObjectContract contract = ResolveContact(objectType);
        contract.DefaultCreator = () => container.GetInstance(objectType);

        return contract;
      }
      //Debug.WriteLine(objectType.FullName + " GetRegistration - false");
      return base.CreateObjectContract(objectType);
    }

    private JsonObjectContract ResolveContact(Type objectType)
    {
      return base.CreateObjectContract(objectType);
    }
  }
}
