using LibraryCore.Models;
using LibraryCore.Models.Repositories;

namespace LibraryCore.ControllerHelpers
{
    public class BranchesControllerUtil
    {
        public const string CheckedOutBranchName = "** checked out **";

        public static string BranchName(IRepository<Branch> branchRepo, int branchId)
        {
            return branchId == Branch.CheckedOutId 
                ? CheckedOutBranchName 
                : branchRepo.GetByID(branchId).Name;
        }
    }
}
