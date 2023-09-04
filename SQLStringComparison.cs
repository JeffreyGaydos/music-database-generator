using System.Collections.Generic;
using System.Globalization;

namespace MusicDatabaseGenerator
{
    public class SQLStringComparison : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == y) return true;
            if (x?.ToLower() == y?.ToLower()) return true;
            return string.Compare(x?.ToLower(), y?.ToLower(), CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0;
        }

        public int GetHashCode(string obj)
        {
            return obj?.ToLower().GetHashCode() ?? 0;
        }
    }
}
