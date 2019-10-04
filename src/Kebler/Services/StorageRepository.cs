using Kebler.Models;
using LiteDB;

namespace Kebler.Services
{
    public static class StorageRepository
    {

        //TODO: Implement storage
        public static LiteCollection<Server> GetServersList()
        {
            LiteCollection<Server> servers;
            using (var db = new LiteDatabase(@"MyData.db"))
            {
                servers = db.GetCollection<Server>(nameof(GetServersList));
            }

            //TODO: Check null expression
            return servers;
        }




    }
}
