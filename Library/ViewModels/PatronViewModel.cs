﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Library.Models;

namespace Library.ViewModels
{
    [NotMapped]
    public class PatronViewModel: Patron
    {
        public PatronViewModel() { }

        public PatronViewModel(Patron patron)
        {
            Id = patron.Id;
            Balance = patron.Balance;
            Name = patron.Name;
        }

        [NotMapped]
        public List<Holding> Holdings { get; set; }
    }
}