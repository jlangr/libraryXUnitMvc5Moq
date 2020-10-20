using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Library.Extensions.SystemWebMvcController
{
    public static class Extensions
    {
        public static string SoleErrorMessage(this Controller controller, string modelKey)
        {
            Console.Write("Keys:");
            foreach (var key in controller.ModelState.Keys)
                Console.Write(" " + key);
            var errors = controller.ModelState[modelKey]?.Errors;
            if (errors != null && errors.Any())
                return errors.First().ErrorMessage;
            return "*** No errors ***";
        }
    }
}