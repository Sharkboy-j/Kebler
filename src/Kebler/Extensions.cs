using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace Kebler
{

    public static class LocalExtensions
    {
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            if (((FrameworkElement)templateFrameworkElement).TemplatedParent is Window window) action(window);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }

        public static bool IsNullOrEmpty(this IEnumerable This)
        {
            return null == This || false == This.GetEnumerator().MoveNext();
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(ToLambda<T>(propertyName));
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderByDescending(ToLambda<T>(propertyName));
        }

        private static Expression<Func<T, object>> ToLambda<T>(string propertyName)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T));
            var property = System.Linq.Expressions.Expression.Property(parameter, propertyName);
            var propAsObject = System.Linq.Expressions.Expression.Convert(property, typeof(object));

            return System.Linq.Expressions.Expression.Lambda<Func<T, object>>(propAsObject, parameter);
        }
    }
}
