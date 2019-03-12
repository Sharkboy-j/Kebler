using Kebler.Models;
using LiteDB;

namespace Kebler.Services
{
    public static class StorageRepository
    {

        //TODO: Implement storage
        public static LiteCollection<Server> ServersList()
        {
            LiteCollection<Server> servers;
            using (var db = new LiteDatabase(@"MyData.db"))
            {
                servers = db.GetCollection<Server>(nameof(ServersList));
            }

            //TODO: Check null expression
            return servers;
        }




    }
}
