using System;
using System.Collections.Generic;

namespace StagWare.System.ExtensionMethods
{
    public static class EnumExtensionMethods
    {
        public static IEnumerable<Enum> GetFlags(this Enum input)
        {
            foreach (Enum value in Enum.GetValues(input.GetType()))
            {
                if (input.HasFlag(value))
                {
                    yield return value;
                }
            }
        }
    }
}
