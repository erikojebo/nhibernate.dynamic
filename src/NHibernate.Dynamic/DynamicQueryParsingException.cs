using System;

namespace NHibernate.Dynamic
{
    public class DynamicQueryParsingException : Exception
    {
        public DynamicQueryParsingException(string message) : base(message) {}
    }
}