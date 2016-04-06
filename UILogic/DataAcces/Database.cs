using Popmail.UILogic.Models;
using SQLite.Net;
using SQLite.Net.Async;
using System;
using System.IO;
using Windows.Storage;

namespace PopMail.DataAcces
{
    public static class Database
    {
        private static SQLiteAsyncConnection dbConnection;

        private static SQLiteAsyncConnection CreateDatabaseAsync()
        {
            // Create a new connection

            try
            {
                var Platform = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();
                var DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path,"Storage.SQLite");
                var ConnectionString = new SQLiteConnectionString(DbPath, true);
                var dbLockedCon = new SQLiteConnectionWithLock(Platform ,ConnectionString);
            
                var db = new SQLiteAsyncConnection(() => dbLockedCon);

                 //Create the tables that do not exist
                Type[] Tables = 
                { 
                       typeof(Folder)
                     , typeof(EmailProvider)
                     , typeof(Email)
                     , typeof(EmailFrom)
                     , typeof(EmailSender)
                     , typeof(EmailReplyTo)
                     , typeof(Settings)
                };
                var d = db.CreateTablesAsync(Tables).Result;
                return db;
            }
            catch(Exception e)
            {
                var mess = e.Message;
                throw e;
            }
        }

        public static SQLiteAsyncConnection DbConnection
        {
            get 
            { 
                if (dbConnection == null)
                {
                    try
                    {
                        dbConnection =  CreateDatabaseAsync();
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
