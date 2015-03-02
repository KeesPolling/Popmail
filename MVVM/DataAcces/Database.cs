using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Windows.Storage;
using PopMailDemo.MVVM.Model;

namespace PopMailDemo.MVVM.DataAcces
{
    public static class Database
    {
        private static string dbPath = string.Empty;
        private static string DbPath
        {
            get
            {
                if (string.IsNullOrEmpty(dbPath))
                {
                    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Storage.sqlite");
                }
                return dbPath;
            }
        }
        private static SQLiteAsyncConnection dbConnection;

        private static async Task<SQLiteAsyncConnection> CreateDatabaseAsync()
        {
            // Create a new connection
            var db = new SQLiteAsyncConnection(DbPath); // TODO Juiste pad uitvinden
            // Create the table if it does not exist
            var d = (db.CreateTableAsync<Folder>().Result).Results; 
            var c = (db.CreateTableAsync<EmailProvider>().Result).Results;

            return db;
        }
  
        public static SQLiteAsyncConnection DbConnection
        {
            get 
            { 
                if (dbConnection == null)
                {
                    try
                    {
                        dbConnection = CreateDatabaseAsync().Result;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                return dbConnection; 
            }
        }
    }
}
