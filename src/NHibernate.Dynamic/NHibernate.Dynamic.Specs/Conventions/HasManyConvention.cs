using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace NHibernate.Dynamic.Specs.Conventions
{
    public class HasManyConvention : IHasManyConvention
    {
        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.Cascade.All();
        }
    }
}