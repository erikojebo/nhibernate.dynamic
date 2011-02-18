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
        private ISession _session;
        private dynamic _repository;

        [SetUp]
        public void SetUp()
        {
            CreateSessionFactory();

            _book1 = new Book { Title = "book 1" };
            _book2 = new Book { Title = "book 2" };
            _book3 = new Book { Title = "book 3" };

            _movie1 = new Movie { Title = "movie 1" };
            _movie2 = new Movie { Title = "movie 2" };
            _movie3 = new Movie { Title = "movie 3" };

            _person1 = new Person { Name = "person 1" };
            _person2 = new Person { Name = "person 2" };

            _person1.FavoriteBooks.Add(_book1);
            _person1.FavoriteBooks.Add(_book2);
            _person2.FavoriteBooks.Add(_book3);

            _person1.FavoriteMovies.Add(_movie1);
            _person1.FavoriteMovies.Add(_movie2);
            _person2.FavoriteMovies.Add(_movie3);

            SaveEntities();

            _session = _factory.OpenSession();
            _repository = new DynamicRepository<Person>(_session);
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
        public void Get_all_loads_all_entities_without_any_relatives()
        {
            IList<Person> actualPersons = _repository.GetAll();

            var firstPerson = actualPersons.FirstOrDefault();

            Assert.AreEqual(2, actualPersons.Count);
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
        public void GetWithCollectionName_without_parameters_returns_all_entities()
        {
            IList<Person> actualPersons = _repository.GetWithFavoriteBooks();

            Assert.AreEqual(2, actualPersons.Count);
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

            Assert.AreEqual(2, actualPersons.Count);
            Assert.AreEqual(_person1.Id, firstPerson.Id);
            Assert.IsTrue(NHibernateUtil.IsInitialized(firstPerson.FavoriteBooks));
            Assert.IsFalse(NHibernateUtil.IsInitialized(firstPerson.FavoriteMovies));
        }

        [Test]
        public void GetAllWithCollectionName_finds_parents_without_children() {}

        [Test]
        public void GetWithChildren_finds_parent_without_children() {}

        [Test]
        public void GetWithManyToOnePropertyName_loads_entity_with_given_relationship_loaded() {}

        [Test]
        public void GetWithManyToOnePropertyName_loads_entity_without_specified_relative() {}

        [Test]
        public void GetAllWithCollectionName1AndCollectionName2_loads_both_collections() {}

        [Test]
        public void GetWithManyToOnePropertynameAndCollectionName_loads_both_relationships() {}

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