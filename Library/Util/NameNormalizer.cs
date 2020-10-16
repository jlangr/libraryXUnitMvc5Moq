using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Antlr.Runtime;

namespace Library.Util
{
    public class NameNormalizer
    {
        public string Normalize(string unnormalizedName)
        {
            ThrowOnTooManyCommas(unnormalizedName);
            var baseAndSuffix = Parse(unnormalizedName);
            var baseName = baseAndSuffix[0];
            var suffix =
                baseAndSuffix.Length > 1
                    ? baseAndSuffix[1]
                    : null;
            var parts = Parts(baseName);
            if (IsMononym(parts))
                return unnormalizedName;
            if (IsDuonym(parts))
                return Last(parts) + ", " + First(parts);
            return Last(parts) + ", " + First(parts) + " " + MiddleInitials(parts) + Suffix(suffix);
        }

        private void ThrowOnTooManyCommas(string name)
        {
            var count = name.Count(c => c == ',');
            if (count > 1) throw new ArgumentException("name can have at most one comma");
        }

        private string Suffix(string suffix)
        {
            if (suffix == null)
                return "";
            return "," + suffix;
        }

        private string[] Parse(string name)
        {
            return name.Split(',');
        }

        private static bool IsDuonym(string[] parts)
        {
            return parts.Length == 2;
        }

        private string MiddleInitials(string[] parts)
        {
            return string.Join(" ", Middles(parts)
                .Select(Initial));
        }

        private IEnumerable<string> Middles(string[] parts)
        {
            return parts.Skip(1).Take(parts.Length - 2);
        }

        private string Initial(string part)
        {
            if (part.Length == 1) return part;
            return part[0] + ".";
        }

        private string[] Parts(string name)
        {
            return name.Trim().Split(' ');
        }

        private static bool IsMononym(string[] parts)
        {
            return parts.Length == 1;
        }

        private static string First(string[] parts)
        {
            return parts.First();
        }

        private static string Last(string[] parts)
        {
            return parts.Last();
        }
    }
}