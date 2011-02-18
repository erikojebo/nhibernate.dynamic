using System.Linq;
using NUnit.Framework;

namespace NHibernate.Dynamic.Specs
{
    [TestFixture]
    public class QuerySpecs
    {
        [Test]
        public void Property_after_GetBy_is_filter()
        {
            var query = new Query("GetByName");

            Assert.AreEqual(1, query.FilterProperties.Count());
            Assert.AreEqual("Name", query.FilterProperties[0]);
        }

        [Test]
        public void Property_filters_can_be_combined_with_and()
        {
            var query = new Query("GetByNameAndAgeAndHomeAddress");

            Assert.AreEqual(3, query.FilterProperties.Count);
            Assert.AreEqual("Name", query.FilterProperties[0]);
            Assert.AreEqual("Age", query.FilterProperties[1]);
            Assert.AreEqual("HomeAddress", query.FilterProperties[2]);
        }

        [Test]
        public void Property_after_With_is_fetch_path()
        {
            var query = new Query("GetWithChildren");

            Assert.AreEqual(1, query.FetchProperties.Count);
            Assert.AreEqual("Children", query.FetchProperties[0]);
        }

        [Test]
        public void Fetch_paths_can_be_combined_with_and()
        {
            var query = new Query("GetWithChildrenAndParentAndOtherChildren");

            Assert.AreEqual(3, query.FetchProperties.Count);
            Assert.AreEqual("Children", query.FetchProperties[0]);
            Assert.AreEqual("Parent", query.FetchProperties[1]);
            Assert.AreEqual("OtherChildren", query.FetchProperties[2]);
        }

        [Test]
        public void Filters_can_be_used_in_combination_with_fetch_paths()
        {
            var query = new Query("GetByNameAndHomeAddressWithChildrenAndOtherChildren");

            Assert.AreEqual(2, query.FilterProperties.Count, "filter properties");
            Assert.AreEqual("Name", query.FilterProperties[0]);
            Assert.AreEqual("HomeAddress", query.FilterProperties[1]);

            Assert.AreEqual(2, query.FetchProperties.Count);
            Assert.AreEqual("Children", query.FetchProperties[0]);
            Assert.AreEqual("OtherChildren", query.FetchProperties[1]);
        }
        
        [Test]
        public void Filters_can_be_used_in_combination_with_fetch_paths_for_unique_query()
        {
            var query = new Query("GetOneByNameAndHomeAddressWithChildrenAndOtherChildren");

            Assert.AreEqual(2, query.FilterProperties.Count, "filter properties");
            Assert.AreEqual("Name", query.FilterProperties[0]);
            Assert.AreEqual("HomeAddress", query.FilterProperties[1]);

            Assert.AreEqual(2, query.FetchProperties.Count);
            Assert.AreEqual("Children", query.FetchProperties[0]);
            Assert.AreEqual("OtherChildren", query.FetchProperties[1]);
        }

        [Test]
        public void GetOne_query_is_unique_query()
        {
            var query = new Query("GetOneByName");
            Assert.IsTrue(query.IsUnique);
        }

        [Test]
        public void Get_query_is_not_unique()
        {
            var query = new Query("GetByName");
            Assert.IsFalse(query.IsUnique);
        }
    }
}