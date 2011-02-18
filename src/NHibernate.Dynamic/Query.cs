using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NHibernate.Dynamic
{
    public class Query
    {
        private readonly List<string> _filterProperties = new List<string>();
        private readonly List<string> _fetchProperties = new List<string>();

        public Query(string query)
        {
            var match = Regex.Match(query, "Get(By(?<filter>.+)(Where(?<fetch>.+))");

            ParseFilters(match.Groups["filter"].Value);

            if (query.StartsWith("GetBy"))
            {
                var filterString = query.Substring("GetBy".Length);

                var filters = filterString.Split(new[] { "And" }, StringSplitOptions.RemoveEmptyEntries);

                _filterProperties.AddRange(filters);
            }

            if (query.StartsWith("GetWith"))
            {
                var fetchString = query.Substring("GetWith".Length);

                var fetchProperties = fetchString.Split(new[] { "And" }, StringSplitOptions.RemoveEmptyEntries);

                _fetchProperties.AddRange(fetchProperties);
            }
        }

        private void ParseFilters(string filter)
        {
        }

        private void ParserFetchPaths(string fetchPats)
        {
            
        }

        public IList<string> FilterProperties
        {
            get { return _filterProperties; }
        }

        public IList<string> FetchProperties
        {
            get { return _fetchProperties; }
        }
    }
}