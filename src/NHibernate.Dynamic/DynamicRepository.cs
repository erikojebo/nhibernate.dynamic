using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace NHibernate.Dynamic
{
    public class DynamicRepository<T> : DynamicObject
        where T : class
    {
        private readonly ISession _session;
        private ICriteria _criteria;
        private Query _query;

        public DynamicRepository(ISession session)
        {
            _session = session;
        }

        public T Get(int id)
        {
            return _session.Get<T>(id);
        }

        public IList<T> GetAll()
        {
            return _session
                .CreateCriteria<T>()
                .List<T>();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            _query = new Query(binder.Name);
            _criteria = _session.CreateCriteria<T>();

            AddFilters(args);

            var isQueryById = args.Length == 1 && !_query.FilterProperties.Any();

            if (isQueryById)
            {
                _criteria.Add(Restrictions.Eq("Id", args[0]));
            }

            if (_query.FetchProperties.Any())
            {
                _criteria.SetFetchMode(_query.FetchProperties.First(), FetchMode.Join);
                _criteria.SetResultTransformer(Transformers.DistinctRootEntity);
            }

            if (_query.IsUnique || isQueryById)
            {
                result = _criteria.UniqueResult<T>();
            }
            else
            {
                result = _criteria.List<T>();
            }

            return true;
        }

        private void AddFilters(object[] args)
        {
            for (int i = 0; i < _query.FilterProperties.Count; i++)
            {
                _criteria.Add(Restrictions.Eq(_query.FilterProperties[i], args[i]));
            }
        }
    }
}