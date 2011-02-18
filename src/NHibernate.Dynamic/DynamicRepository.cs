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

            AddFilters(args);
            AddIdRestriction(args);
            FetchRelatives();

            result = GetResult();

            return true;
        }

        private void AddIdRestriction(object[] args)
        {
            if (IsQueryById)
            {
                _criteria.Add(Restrictions.Eq("Id", args[0]));
            }
        }

        private bool IsQueryById
        {
            get { return _args.Length == 1 && !_query.FilterProperties.Any(); }
        }

        private void FetchRelatives()
        {
            if (_query.FetchProperties.Any())
            {
                _criteria.SetFetchMode(_query.FetchProperties.First(), FetchMode.Join);
                _criteria.SetResultTransformer(Transformers.DistinctRootEntity);
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

        private void AddFilters(object[] args)
        {
            for (int i = 0; i < _query.FilterProperties.Count; i++)
            {
                _criteria.Add(Restrictions.Eq(_query.FilterProperties[i], args[i]));
            }
        }
    }
}