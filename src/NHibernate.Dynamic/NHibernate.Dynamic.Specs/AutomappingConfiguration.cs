using FluentNHibernate.Automapping;
using NHibernate.Dynamic.Specs.Entities;

namespace NHibernate.Dynamic.Specs
{
    public class AutomappingConfiguration : DefaultAutomappingConfiguration
    {
        public override bool AbstractClassIsLayerSupertype(System.Type type)
        {
            return type == typeof(Entity);
        }

        public override bool ShouldMap(System.Type type)
        {
            return typeof(Entity).IsAssignableFrom(type);
        }
    }
}