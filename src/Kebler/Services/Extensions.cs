using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Kebler.Services
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty(this IEnumerable This)
        {
            return null == This || false == This.GetEnumerator().MoveNext();
        }
    }
}
