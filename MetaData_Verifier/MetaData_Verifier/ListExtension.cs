using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaData_Verifier
{
    public static class ListExtension
    {
        public static void Add<T>(this List<T> obj, T[] items)
        {
            foreach (T item in items)
                obj.Add(item);
        }

    }
}
