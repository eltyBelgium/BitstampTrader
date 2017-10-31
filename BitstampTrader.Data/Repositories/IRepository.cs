using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitstampTrader.Data.Repositories
{
    interface IRepository<T>
    {
        void Add(T newEntity);
        void Save();
        void Update(T entity);
        IEnumerable<T> Where(Func<T, bool> predicate);
        List<T> ToList();
    }
}
