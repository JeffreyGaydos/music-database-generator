using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace PlaylistTransferTool
{
    public class UniversalPathComparator: IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == y) return true;
            if (x?.ToLower() == y?.ToLower()) return true;
            x.Replace('\\', '/');
            y.Replace('\\', '/');
            return string.Compare(x?.ToLower(), y?.ToLower(), CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0;
        }

        public int GetHashCode(string obj)
        {
            return obj?.Replace('\\', '/').ToLower().GetHashCode() ?? 0;
        }

        public string SanitizeString(string s)
        {
            return s.Replace('\\', '/').ToLower();
        }

        public string ToWindowsPath(string s)
        {
            return s.Replace('/', '\\');
        }
    }
}
