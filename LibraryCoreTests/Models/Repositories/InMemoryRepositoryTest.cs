using System;
using System.Linq;
using Library.Models;
using Library.Models.Repositories;
using Xunit;
using Assert = Xunit.Assert;

namespace LibraryTests.LibraryTest.Models.Repositories
{
    public class InMemoryRepositoryTest
    {
        private X x;
        private InMemoryRepository<X> repo;

        [Serializable]
        private class X : Identifiable {
            public int Id { get; set; }
            public string Name { get; set; }
        };
        
        public InMemoryRepositoryTest()
        {
            x = new X();
            repo = new InMemoryRepository<X>();
        }

        [Fact]
        public void InitialIdIs1()
        {
            var id = repo.Create(x);

            Assert.Equal(1, id);
        }

        [Fact]
        public void IdIncrementsOnCreate()
        {
            repo.Create(x);

            var id = repo.Create(x);

            Assert.Equal(2, id);
        }

        [Fact]
        public void RetrievedInstanceNotSameAsCreated()
        {
            var id = repo.Create(x);

            var retrieved = repo.GetByID(id);

            Assert.NotSame(x, retrieved);
        }

        [Fact]
        public void FindsUsingLambda()
        {
            var alpha = new X { Name = "alpha" };
            var beta = new X { Name = "beta" };
            repo.Create(alpha);
            var betaId = repo.Create(beta);

            var retrieved = repo.Get(each => each.Name == "beta");

            Assert.Equal(betaId, retrieved.Id);
        }

        [Fact]
        public void ClearsRepo()
        {
            repo.Create(x);

            repo.Clear();

            Assert.Empty(repo.GetAll());
        }

        [Fact]
        public void RetrieveWithoutSaveReturnsOriginallySavedVersion()
        {
            x.Name = "original";
            repo.Create(x);
            x.Name = "new";

            var retrieved = repo.GetByID(x.Id);

            Assert.Equal("original", retrieved.Name);
        }

        [Fact]
        public void RetrieveAfterSaveReturnsUpdatedEntity()
        {
            x.Name = "original";
            repo.Create(x);
            x.Name = "new";
            repo.Save(x);

            var retrieved = repo.GetByID(x.Id);

            Assert.Equal("new", retrieved.Name);
        }

        [Fact]
        public void MarkModifiedDoesNothingIfSaveNotCalled()
        {
            x.Name = "original";
            repo.Create(x);
            x.Name = "new";

            repo.MarkModified(x);

            var retrieved = repo.GetByID(x.Id);
            Assert.Equal("original", retrieved.Name);
        }

        [Fact]
        public void SavePersistsEntitiesMarkedModified()
        {
            x.Name = "original";
            repo.Create(x);
            x.Name = "new";
            repo.MarkModified(x);

            repo.Save();

            var retrieved = repo.GetByID(x.Id);
            Assert.Equal("new", retrieved.Name);
        }

        [Fact]
        public void ModifiedEntitiesClearedAfterSave()
        {
            repo.Create(x);
            x.Name = "First Save";
            repo.MarkModified(x);
            repo.Save();

            x.Name = "After First Save";
            repo.Save();

            var retrieved = repo.GetByID(x.Id);
            Assert.Equal("First Save", retrieved.Name);
        }

        [Fact]
        public void ModifiedEntitiesClearedAfterClear()
        {
            repo.MarkModified(x);

            repo.Clear();

            Assert.False(repo.ModifiedEntities.Any());
        }
    }
}
