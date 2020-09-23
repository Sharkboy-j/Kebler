using Kebler.Models;
using LiteDB;

namespace Kebler.Services
{
	public static class StorageRepository
	{
		public static LiteCollection<Server> GetServersList()
		{
            using var db = new LiteDatabase(Const.ConstStrings.GetDataDBFilePath);
            var servers = db.GetCollection<Server>(nameof(GetServersList));
            return servers;
		}
	}
}
