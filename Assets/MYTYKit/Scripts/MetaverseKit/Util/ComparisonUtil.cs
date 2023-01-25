using System;
using System.Globalization;
using System.Numerics;

namespace MYTYKit.Scripts.MetaverseKit.Util
{
    public class ComparisonUtil
    {
        public static int CompareStrings(string a, string b)
        {
            BigInteger aVal, bVal;
            var aRes = BigInteger.TryParse(a, NumberStyles.Number, null, out aVal);
            var bRes = BigInteger.TryParse(b, NumberStyles.Number, null, out bVal);
            return aRes switch
            {
                true when !bRes => -1,
                false when bRes => 1,
                false when !bRes => String.Compare(a, b, StringComparison.Ordinal),
                _ => aVal.CompareTo(bVal)
            };
        }
    }
}