using System.Collections.Generic;
using System.Dynamic;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace NHibernate.Dynamic
{
    public class DynamicRepository<T> : DynamicObject
        where T : class
    {
        private readonly ISession _session;
        private string _calledMethodName;

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
            result = null;

            _calledMethodName = binder.Name;

            if (_calledMethodName.StartsWith("GetBy"))
            {
                result = FindByProperty(args);
            }
            else if (_calledMethodName.StartsWith("GetWith"))
            {
                result = GetWithChildren(args);
            }
            else
            {
                return false;
            }

            return true;
        }

        private object GetWithChildren(object[] args)
        {
            var propertyName = _calledMethodName.Substring("GetWith".Length);

            return _session.CreateCriteria<T>()
                .Add(Restrictions.Eq("Id", args[0]))
                .SetFetchMode(propertyName, FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .UniqueResult<T>();
        }

        private object FindByProperty(object[] args)
        {
            var propertyName = _calledMethodName.Substring("GetBy".Length);

            return _session.CreateCriteria<T>()
                .Add(Restrictions.Eq(propertyName, args[0]))
                .UniqueResult<T>();
        }
    }
}