using Library.ControllerHelpers;
using Library.Models;
using Library.Models.Repositories;
using Xunit;
using Assert = Xunit.Assert;

namespace LibraryCoreTests.ControllerHelpers
{
    public class BranchesControllerUtilTest
    {
        [Fact]
        public void BranchNameForCheckedOutBranch()
        {
            Assert.Equal(BranchesControllerUtil.CheckedOutBranchName, 
                BranchesControllerUtil.BranchName(new InMemoryRepository<Branch>(), Branch.CheckedOutId));
        }

        [Fact]
        public void BranchNameForBranch()
        {
            var branchRepo = new InMemoryRepository<Branch>();
            var branchId = branchRepo.Create(new Branch { Name = "NewBranchName" });

            var branchName = BranchesControllerUtil.BranchName(branchRepo, branchId);

            Assert.Equal("NewBranchName", branchName);
        }
    }
}
