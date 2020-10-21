using System.Collections.Generic;
using LibraryCore.Models;

namespace LibraryCore.ViewModels
{
    public class CheckInViewModel
    {
        public string Barcode { get; set; }
        public List<Branch> BranchesViewList { get; internal set; }
        public int BranchId { get; set; }
    }
}