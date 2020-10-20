using System.Linq;
using Library.Models;
using Library.Models.Repositories;
using System.Collections.Generic;
using Xunit;
using Assert = Xunit.Assert;

namespace LibraryTests.LibraryTest.Models.Repositories
{
    public class BranchRepositoryExtensionsTest
    {
        [Fact]
        public void PrependsCheckedOutBranchToListOfAllBranches()
        {
            var branchRepo = new InMemoryRepository<Branch>();
            branchRepo.Create(new Branch { Name = "A" });
            branchRepo.Create(new Branch { Name = "B" });

            var branches = branchRepo.GetAllIncludingCheckedOutBranch();

            Assert.Equal(Branch.CheckedOutBranch.Name, branches.First().Name);
            Assert.Equal(new List<string> { "A", "B" }, branches.Skip(1).Select(b => b.Name));
            
        }
    }
}
