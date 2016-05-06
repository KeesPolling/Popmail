using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;
using Popmail.UILogic.DataAcces;
using Popmail.UILogic.Models;

namespace Popmail.UILogic.ViewModels
{
     public class SettingsMethods
    {
        public static async Task<string> GetSetting(string name)
        {
            var db = Database.DbConnection;
            var result = await db.FindAsync<Settings>(s => s.Name == name) ;
            return result?.Value;
        }

        public static async Task SetSetting(string name, string value)
        {
            var db = Database.DbConnection;
            var current = await db.FindAsync<Settings>(s => s.Name == name);
            if (current == null)
            {
                var setting = new Settings() {Name =name, Value = value};
                var result = db.InsertAsync(setting);
                return;;
            }
            current.Value = value;
            var updateResult = db.UpdateAsync(current);
            return;
        }
        public static async Task RemoveSetting(string name)
        {
            var db = Database.DbConnection;
            var current = await db.FindAsync<Settings>(s => s.Name == name);
            if (current == null) return;
            var removeResult = db.DeleteAsync<Settings>(current.Id);
            return;
        }
    }
}
