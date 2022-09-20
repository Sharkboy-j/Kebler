using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Kebler.Core.Models;
using Kebler.Services;
using Kebler.TransmissionTorrentClient.Models;
using NLog;
using Expression = System.Linq.Expressions.Expression;

namespace Kebler
{
    public static class LocalExtensions
    {
        private static ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            if (((FrameworkElement)templateFrameworkElement).TemplatedParent is Window window) action(window);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
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
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, propertyName);
            var propAsObject = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
        }


        public static void ParseTransmissionReponse(this TransmissionResponse resp)
        {
            if (ConfigService.Instanse.TraceRequestsEnabled)
            {
                if (resp.Success)
                {
                    logger.Info($"[{resp.Method}] RESULT '{resp.Result}'");


                }
                else
                {
                    var sb = new StringBuilder();
                    if (resp.HttpWebResponse != null)
                        sb.Append(resp.HttpWebResponse).Append(Environment.NewLine);

                    if (resp.WebException != null)
                        sb.Append(resp.WebException).Append(Environment.NewLine);

                    if (resp.CustomException != null)
                        sb.Append(resp.CustomException).Append(Environment.NewLine);

                    logger.Error($"[{resp.Method}] RESULT '{resp.Result}'{Environment.NewLine}{sb}");
                }

            }
        }

        public static void ParseTransmissionReponse<T>(this TransmissionResponse<T> resp)
        {
            resp.Response.ParseTransmissionReponse();
        }


    }

}