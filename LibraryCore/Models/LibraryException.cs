using System;

namespace LibraryCore.Models
{
    public class LibraryException: ApplicationException
    {
        public LibraryException(string message) : base(message)
        {
        }
    }
}
