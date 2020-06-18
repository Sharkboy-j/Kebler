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
            Stack<IEnumerator<T>> stack = new Stack<IEnumerator<T>>();
            try
            {
                stack.Push(input.GetEnumerator());
                while (stack.Count > 0)
                {
                    while (stack.Peek().MoveNext())
                    {
                        T element = stack.Peek().Current;
                        yield return element;
                        IEnumerable<T> objs = recursion(element);
                        if (objs != null)
                            stack.Push(objs.GetEnumerator());
                        element = default(T);
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
