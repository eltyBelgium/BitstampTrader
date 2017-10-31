using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitstampTrader.Data.Repositories
{
    public class SqlRepository<T> : IRepository<T> where T:class 
    {
        private readonly DbContext _ctx;

        public SqlRepository(DbContext context)
        {
            _ctx = context;
        }

        public void Add(T newEntity)
        {
            _ctx.Set<T>().Add(newEntity);
        }

        public void Save()
        {
            _ctx.SaveChanges();
        }

        public void Update(T entity)
        {
            DbEntityEntry entityEntry = _ctx.Entry(entity);
            if (entityEntry.State == EntityState.Detached)
            {
                _ctx.Set<T>().Attach(entity);
                entityEntry.State = EntityState.Modified;
            }
        }

        public IEnumerable<T> Where(Func<T, bool> predicate)
        {
            return _ctx.Set<T>().Where(predicate);
        }

        public List<T> ToList()
        {
            return _ctx.Set<T>().ToList();
        }
    }
}
