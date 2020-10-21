using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LibraryCore.Models;

namespace LibraryCore.ViewModels
{
    public class CheckOutViewModel
    {
        [DisplayName("Branch")]
        public List<Branch> BranchesViewList { get; set; }
        [DisplayName("Holding Barcode"), Required()]
        public string Barcode { get; set; }
        [DisplayName("Patron ID"), Required()]
        public int PatronId { get; set; }
    }
}