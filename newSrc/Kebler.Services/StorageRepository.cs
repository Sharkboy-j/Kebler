using System.IO;
using Kebler.Domain;
using Kebler.Domain.Interfaces;
using Kebler.Domain.Models;
using LiteDB;

namespace Kebler.Services
{
    public static class StorageRepository
    {
        public static LiteCollection<Server> GetServersList()
        {
            var dbPath = Path.Combine(ConstStrings.GetDataPath().FullName, $"{nameof(Kebler)}.db");


            using var db = new LiteDatabase(dbPath);
            var servers = db.GetCollection<Server>(nameof(GetServersList));

            return servers;
        }
    }
}