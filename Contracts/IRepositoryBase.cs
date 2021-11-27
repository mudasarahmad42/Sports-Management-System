using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Contracts
{
    public interface IRepositoryBase<T> where T : class 
    {
        ICollection<T> FindAll();
        T FindbyId(int id);
        bool Create(T entity);
        bool Delete(T entity);
        bool Update(T entity);
        bool Save();
    }
}
