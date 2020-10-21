using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using LibraryCore.Models;

namespace LibraryCore.ViewModels
{
    [NotMapped]
    public class PatronViewModel: Patron
    {
        public PatronViewModel() { }

        public PatronViewModel(Patron patron)
        {
            this.Id = patron.Id;
            this.Balance = patron.Balance;
            this.Name = patron.Name;
        }

        [NotMapped]
        public List<Holding> Holdings { get; set; }
    }
}