using System.Collections.Generic;

namespace NHibernate.Dynamic.Specs.Entities
{
    public class Person : Entity
    {
        public Person()
        {
            FavoriteBooks = new List<Book>();
            FavoriteMovies = new List<Movie>();
        }

        public virtual string Name { get; set; }
        public virtual IList<Book> FavoriteBooks { get; set; }
        public virtual IList<Movie> FavoriteMovies { get; set; }
        public virtual Person BFF { get; set; }
    }
}