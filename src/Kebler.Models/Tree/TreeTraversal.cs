using System;
using System.Collections.Generic;

namespace Kebler.Models.Tree
{
    internal static class TreeTraversal
    {
        public static IEnumerable<T> PreOrder<T>(T root, Func<T, IEnumerable<T>> recursion)
        {
            return PreOrder(new T[1]
            {
                root
            }, recursion);
        }

        public static IEnumerable<T> PreOrder<T>(IEnumerable<T> input, Func<T, IEnumerable<T>> recursion)
        {
            var stack = new Stack<IEnumerator<T>>();
            try
            {
                stack.Push(input.GetEnumerator());
                while (stack.Count > 0)
                {
                    while (stack.Peek().MoveNext())
                    {
                        var element = stack.Peek().Current;
                        yield return element;
                        var objs = recursion(element);
                        if (objs != null)
                            stack.Push(objs.GetEnumerator());
                    }

                    stack.Pop().Dispose();
                }
            }
            finally
            {
                while (stack.Count > 0)
                    stack.Pop().Dispose();
            }
        }
    }
}