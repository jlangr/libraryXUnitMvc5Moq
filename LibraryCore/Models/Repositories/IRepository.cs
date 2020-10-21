using System;
using System.Collections.Generic;

namespace LibraryCore.Models.Repositories
{
    public interface IRepository<T>
        where T: Identifiable
    {
        void Clear();
        int Create(T entity);
        void Delete(int id);
        IEnumerable<T> FindBy(Func<T, bool> predicate);
        T GetByID(int id);
        IEnumerable<T> GetAll();
        int Save();
        void Save(T entity);
        void MarkModified(T entity);
        void Dispose();
    }
}
