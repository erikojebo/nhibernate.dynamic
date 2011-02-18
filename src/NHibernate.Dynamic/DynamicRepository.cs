using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NHibernate.Criterion;

namespace NHibernate.Dynamic
{
    public class DynamicRepository<T> : DynamicObject
        where T : class
    {
        private readonly ISession _session;
        private ICriteria _criteria;
        private Query _query;
        private object[] _args;

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
            _args = args;
            _query = new Query(binder.Name);
            _criteria = _session.CreateCriteria<T>();

            AddFilters(_criteria);
            AddIdRestriction(_criteria);

            result = GetResult();

            FetchRelatives();

            return true;
        }

        private bool IsQueryById
        {
            get { return _args.Length == 1 && !_query.FilterProperties.Any(); }
        }

        private void FetchRelatives()
        {
            foreach (var fetchProperty in _query.FetchProperties)
            {
                var fetchCriteria = _session.CreateCriteria<T>()
                    .SetFetchMode(fetchProperty, FetchMode.Join);

                AddFilters(_criteria);
                AddIdRestriction(_criteria);

                fetchCriteria.List<T>();
            }
        }

        private object GetResult()
        {
            if (_query.IsUnique || IsQueryById)
            {
                return _criteria.UniqueResult<T>();
            }

            return _criteria.List<T>();
        }

        private void AddFilters(ICriteria criteria)
        {
            for (int i = 0; i < _query.FilterProperties.Count; i++)
            {
                criteria.Add(Restrictions.Eq(_query.FilterProperties[i], _args[i]));
            }
        }

        private void AddIdRestriction(ICriteria criteria)
        {
            if (IsQueryById)
            {
                criteria.Add(Restrictions.Eq("Id", _args[0]));
            }
        }
    }
}