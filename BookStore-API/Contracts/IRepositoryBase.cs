using BookStore_API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Contracts
{
    public interface IRepositoryBase<T> where T : class
    {
        Task<IList<T>> FindAll();

        Task<T> FindById(int id);

        Task<bool> Update(Author entity);
        Task<bool> Save();
        Task<bool> Delete(Author entity);
        Task<bool> Create(Author entity);

    }
}
