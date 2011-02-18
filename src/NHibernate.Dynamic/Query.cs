using System;
using System.Collections.Generic;

namespace NHibernate.Dynamic
{
    public class Query
    {
        private readonly List<string> _filterProperties = new List<string>();
        private readonly List<string> _fetchProperties = new List<string>();
        private string _remainingQuery;

        public Query(string query)
        {
            if (!query.StartsWith("Get"))
            {
                return;
            }

            _remainingQuery = query.Substring(3);

            if (_remainingQuery.StartsWith("One"))
            {
                IsUnique = true;
                _remainingQuery = _remainingQuery.Substring(3);
            }

            ParseFilter();
            ParseFetchPaths();
        }

        private void ParseFilter()
        {
            if (!_remainingQuery.StartsWith("By"))
            {
                return;
            }

            _remainingQuery = _remainingQuery.Substring(2);

            if (_remainingQuery.Contains("With"))
            {
                var whereIndex = _remainingQuery.IndexOf("With");

                var filterString = _remainingQuery.Substring(0, whereIndex);
                _filterProperties.AddRange(Split(filterString, "And"));

                _remainingQuery = _remainingQuery.Substring(whereIndex);
            }
            else
            {
                _filterProperties.AddRange(Split(_remainingQuery, "And"));
            }
        }

        private void ParseFetchPaths()
        {
            if (!_remainingQuery.StartsWith("With"))
            {
                return;
            }

            _remainingQuery = _remainingQuery.Substring(4);

            _fetchProperties.AddRange(Split(_remainingQuery, "And"));
        }

        private string[] Split(string s, params string[] words)
        {
            return s.Split(words, StringSplitOptions.None);
        }

        public IList<string> FilterProperties
        {
            get { return _filterProperties; }
        }

        public IList<string> FetchProperties
        {
            get { return _fetchProperties; }
        }

        public bool IsUnique { get; private set; }
    }
}