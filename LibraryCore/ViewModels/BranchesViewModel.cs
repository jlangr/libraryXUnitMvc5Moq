using System.Collections;
using LibraryCore.Models;

namespace LibraryCore.ViewModels
{
    public class BranchesViewModel
    {
        public Branch SelectedBranch { get; set; }
        public IEnumerable Branches { get; set; }
    }
}