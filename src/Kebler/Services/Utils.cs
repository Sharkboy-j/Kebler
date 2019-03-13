using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kebler.Models;

namespace Kebler.Services
{
    public static class Utils
    {

        public static void LogServers<T>(this List<T> serverList)
        {
            if (serverList.Count == 0)
            {
                App.Log.Info("ServerListCount: 0");
            }
            var txt = $"ServersList[{serverList.Count}]:{Environment.NewLine}";
            foreach (var server in serverList)
            {
                txt += server + Environment.NewLine;
            }
            App.Log.Info(txt);
        }
    }
}
