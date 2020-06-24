using System.IO;
using Kebler.Models;
using LiteDB;

namespace Kebler.Services
{
	public static class StorageRepository
	{

		public static LiteCollection<Server> GetServersList()
		{
			var pth = Path.Combine(Data.GetDataPath().FullName, $"{nameof(Kebler)}.db");


            using var db = new LiteDatabase(pth);
            var servers = db.GetCollection<Server>(nameof(GetServersList));
            return servers;
		}




	}
}
