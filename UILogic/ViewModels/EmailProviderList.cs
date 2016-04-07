using Popmail.UILogic.Models;
using System.Collections.Generic;
using Popmail.UILogic.DataAcces;

namespace Popmail.UILogic.ViewModels
{
    public static class EmailProviderList
    {
        public static List<EmailProviderPageViewModel> EmailProviders
        {
            get
            {
                var db = Database.DbConnection;
                var ProviderTable = db.Table<Accounts>().ToListAsync().Result;
                var emailProviders = new List<EmailProviderPageViewModel>();
                foreach (var Provider in ProviderTable)
                {
                    emailProviders.Add(new EmailProviderPageViewModel(null));
                }
                return emailProviders;
            }
        }
    }
}
