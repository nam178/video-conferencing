using System.Collections.Generic;
using System.Linq;

namespace MediaServer.Common.Utils
{
    static class CopyOnWrite
    {
        public static void Add<T>(ref IReadOnlyList<T> original, T newElement, object sync)
        {
            lock(sync)
            {
                var t = original.ToList();
                t.Add(newElement);
                original = t;
            }
        }
    }
}
