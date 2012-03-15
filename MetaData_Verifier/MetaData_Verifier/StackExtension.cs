using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaData_Verifier
{
    public static class StackExtension
    {
        public static void Push<T>(this Stack<T> obj, T[] items)
        {
            for(int i = items.Length -1; i >=0; --i)
                obj.Push(items[i]);
        }
    }
}
