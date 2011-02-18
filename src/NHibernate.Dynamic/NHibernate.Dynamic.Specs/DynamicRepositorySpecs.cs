using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Dynamic.Specs.Conventions;
using NHibernate.Dynamic.Specs.Entities;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;

namespace NHibernate.Dynamic.Specs
{
    [TestFixture]
    public class DynamicRepositorySpecs
    {
        private ISessionFactory _factory;
        private Book _book1;
        private Book _book2;
        private Movie _movie1;
        private Movie _movie2;
        private Movie _movie3;
        private Book _book3;
        private Person _person2;
        private Person _person1;
        private Person _person3;
        private ISession _session;
        private dynamic _repository;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            CreateSessionFactory();
        }

        [SetUp]
        public void SetUp()
        {
            _session = _factory.OpenSession();
            _repository = new DynamicRepository<Person>(_session);

            _session.CreateQuery("delete from Book").ExecuteUpdate();
            _session.CreateQuery("delete from Movie").ExecuteUpdate();
            _session.CreateQuery("delete from Person").ExecuteUpdate();

            _book1 = new Book { Title = "book 1" };
            _book2 = new Book { Title = "book 2" };
            _book3 = new Book { Title = "book 3" };

            _movie1 = new Movie { Title = "movie 1" };
            _movie2 = new Movie { Title = "movie 2" };
            _movie3 = new Movie { Title = "movie 3" };

            _person1 = new Person { Name = "person 1" };
            _person2 = new Person { Name = "person 2" };
            _person3 = new Person { Name = "person 3" };

            _person1.FavoriteBooks.Add(_book1);
            _person1.FavoriteBooks.Add(_book2);
            _person2.FavoriteBooks.Add(_book3);

            _person1.FavoriteMovies.Add(_movie1);
            _person1.FavoriteMovies.Add(_movie2);
            _person2.FavoriteMovies.Add(_movie3);

            _person1.BFF = _person2;
            _person2.BFF = _person1;

            SaveEntities();
        }

        [Test]
        public void Get_loads_entity_without_any_relatives()
        {
            Person actualPerson = _repository.Get(_person1.Id);

            Assert.IsNotNull(actualPerson);
            Assert.AreEqual("person 1", actualPerson.Name);
            Assert.IsFalse(NHibernateUtil.IsInitialized(actualPerson.FavoriteBooks));
            Assert.IsFalse(NHibernateUtil.IsInitialized(actualPerson.FavoriteMovies));
        }

        [Test]
        public void Get_all_loads_all_entities_without_any_relatives_loaded_including_entities_without_relatives()
        {
            IList<Person> actualPersons = _repository.GetAll();

            var firstPerson = actualPersons.FirstOrDefault();

            Assert.AreEqual(3, actualPersons.Count);
            Assert.AreEqual("person 1", firstPerson.Name);
            Assert.IsFalse(NHibernateUtil.IsInitialized(firstPerson.FavoriteBooks));
            Assert.IsFalse(NHibernateUtil.IsInitialized(firstPerson.FavoriteMovies));
        }

        [Test]
        public void GetWithCollectionName_given_one_argument_queries_by_id()
        {
            Person actualPerson = _repository.GetWithFavoriteBooks(_person1.Id);

            Assert.IsNotNull(actualPerson);
            Assert.AreEqual(_person1.Id, actualPerson.Id);
        }

        [Test]
        public void GetWithCollectionName_without_parameters_returns_all_entities_including_those_without_children()
        {
            IList<Person> actualPersons = _repository.GetWithFavoriteBooks();

            Assert.AreEqual(3, actualPersons.Count);
        }

        [Test]
        public void GetWithCollectionName_loads_corresponding_child_collection()
        {
            Person actualPerson = _repository.GetWithFavoriteBooks(_person1.Id);

            Assert.IsNotNull(actualPerson);
            Assert.AreEqual(_person1.Id, actualPerson.Id);
            Assert.IsTrue(NHibernateUtil.IsInitialized(actualPerson.FavoriteBooks));
            Assert.IsFalse(NHibernateUtil.IsInitialized(actualPerson.FavoriteMovies));
        }

        [Test]
        public void GetWithCollectionName_loads_all_with_corresponding_child_collection_without_duplicates()
        {
            IList<Person> actualPersons = _repository.GetWithFavoriteBooks();

            var firstPerson = actualPersons.FirstOrDefault();

            Assert.AreEqual(3, actualPersons.Count);
            Assert.AreEqual(_person1.Id, firstPerson.Id);
            Assert.IsTrue(NHibernateUtil.IsInitialized(firstPerson.FavoriteBooks));
            Assert.IsFalse(NHibernateUtil.IsInitialized(firstPerson.FavoriteMovies));
        }

        [Test]
        public void GetWithChildren_when_unique_finds_parent_without_children()
        {
            Person actualPerson = _repository.GetWithFavoriteBooks(_person3.Id);

            Assert.IsNotNull(actualPerson);
            Assert.AreEqual(_person3.Id, actualPerson.Id);
            Assert.IsTrue(NHibernateUtil.IsInitialized(actualPerson.FavoriteBooks));
        }

        [Test]
        public void GetWithManyToOnePropertyName_loads_entity_with_given_relationship_loaded()
        {
            Person actualPerson = _repository.GetWithBFF(_person1.Id);

            Assert.IsNotNull(actualPerson);
            Assert.AreEqual(_person1.Id, actualPerson.Id);
            Assert.IsTrue(NHibernateUtil.IsInitialized(actualPerson.BFF));
        }

        [Test]
        public void GetWithManyToOnePropertyName_loads_entity_without_specified_relative()
        {
            Person actualPerson = _repository.GetWithBFF(_person3.Id);

            Assert.IsNotNull(actualPerson);
            Assert.AreEqual(_person3.Id, actualPerson.Id);
            Assert.IsTrue(NHibernateUtil.IsInitialized(actualPerson.BFF));
        }

        [Test]
        public void GetWithCollectionName1AndCollectionName2_loads_both_collections()
        {
            Person actualPerson = _repository.GetWithFavoriteBooksAndFavoriteMovies(_person1.Id);

            Assert.IsNotNull(actualPerson);
            Assert.AreEqual(_person1.Id, actualPerson.Id);
            Assert.IsTrue(NHibernateUtil.IsInitialized(actualPerson.FavoriteBooks));
            Assert.IsTrue(NHibernateUtil.IsInitialized(actualPerson.FavoriteMovies));
        }

        [Test]
        public void GetWithManyToOnePropertynameAndCollectionName_loads_both_relationships()
        {
            IList<Person> actualPersons = _repository.GetWithFavoriteBooksAndFavoriteMovies();

            var firstPerson = actualPersons.FirstOrDefault();

            Assert.AreEqual(3, actualPersons.Count);
            Assert.AreEqual(_person1.Id, firstPerson.Id);
            Assert.IsTrue(NHibernateUtil.IsInitialized(firstPerson.FavoriteBooks));
            Assert.IsTrue(NHibernateUtil.IsInitialized(firstPerson.BFF));
        }

        [Test]
        public void GetByPropertyName_filters_on_given_property()
        {
            Person actualPerson = _repository.GetOneByName("person 1");

            Assert.IsNotNull(actualPerson);
            Assert.AreEqual("person 1", actualPerson.Name);
            Assert.IsFalse(NHibernateUtil.IsInitialized(actualPerson.FavoriteBooks));
            Assert.IsFalse(NHibernateUtil.IsInitialized(actualPerson.FavoriteMovies));
        }

        private void SaveEntities()
        {
            using (var session = _factory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.Save(_person1);
                session.Save(_person2);
                session.Save(_person3);

                transaction.Commit();
            }
        }

        private void CreateSessionFactory()
        {
            _factory = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2005
                    .ConnectionString(
                        x => x.Server(".\\SQLEXPRESS")
                                 .Database("nhibernate.dynamic")
                                 .TrustedConnection())
                    .ShowSql)
                .Mappings(m => m.AutoMappings.Add(
                    AutoMap.Assemblies(
                        new AutomappingConfiguration(),
                        typeof(Person).Assembly)
                                   .Conventions.AddFromAssemblyOf<HasManyConvention>()))
                .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(true, true))
                .BuildSessionFactory();
        }
    }
}