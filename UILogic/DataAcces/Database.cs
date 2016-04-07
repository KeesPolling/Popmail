using System;
using System.IO;
using Windows.Storage;
using Popmail.UILogic.Models;
using SQLite.Net;
using SQLite.Net.Async;

namespace Popmail.UILogic.DataAcces
{
    public static class Database
    {
        private static SQLiteAsyncConnection _dbConnection;

        private static SQLiteAsyncConnection CreateDatabaseAsync()
        {
            // Create a new connection

            try
            {
                var platform = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();
                var dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path,"Storage.SQLite");
                var connectionString = new SQLiteConnectionString(dbPath, true);
                var dbLockedCon = new SQLiteConnectionWithLock(platform ,connectionString);
            
                var db = new SQLiteAsyncConnection(() => dbLockedCon);

                 //Create the tables that do not exist
                Type[] tables = 
                { 
                       typeof(Folders)
                     , typeof(Accounts)
                     , typeof(Emails)
                     , typeof(EmailFrom)
                     , typeof(EmailSender)
                     , typeof(EmailReplyTo)
                     , typeof(Settings)
                };
                var d = db.CreateTablesAsync(tables).Result;
                return db;
            }
            catch(Exception e)
            {
                var mess = e.Message;
                throw;
            }
        }

        public static SQLiteAsyncConnection DbConnection
        {
            get 
            { 
                if (_dbConnection == null)
                {
                    try
                    {
                        _dbConnection =  CreateDatabaseAsync();
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                }
                return _dbConnection; 
            }
        }
    }
}
