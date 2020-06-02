using Kebler.Models;
using LiteDB;
using System;
using System.IO;

namespace Kebler.Services
{
	public static class StorageRepository
	{

		public static LiteCollection<Server> GetServersList()
		{
			var pth = Path.Combine(Data.GetDataPath().FullName, $"{nameof(Kebler)}.db");
			

			LiteCollection<Server> servers;
			using (var db = new LiteDatabase(pth))
			{
				servers = db.GetCollection<Server>(nameof(GetServersList));
			}

			//TODO: Check null expression
			return servers;
		}




	}
}
