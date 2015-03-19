﻿using PopMailDemo.MVVM.Model;
using SQLite.Net;
using SQLite;
using System;
using System.IO;
using Windows.Storage;
using SQLite.Net.Interop;
using SQLite.Net.Platform.WinRT;
using SQLite.Net.Async;

namespace PopMailDemo.MVVM.DataAcces
{
    public static class Database
    {
        private static SQLiteAsyncConnection dbConnection;

        private static SQLiteAsyncConnection CreateDatabaseAsync()
        {
            // Create a new connection

            try
            {
                var Platform = new SQLitePlatformWinRT();
                var DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path,"Storage.SQLite");
                var ConnectionString = new SQLiteConnectionString(DbPath, true);
                var dbLockedCon = new SQLiteConnectionWithLock(Platform ,ConnectionString);
            
                var db = new SQLiteAsyncConnection(() => dbLockedCon);

                 //Create the table if it does not exist
                Type[] Tables = 
                { 
                       typeof(Folder)
                     , typeof(EmailProvider)
                     , typeof(Email)
                     , typeof(EmailFrom)
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
