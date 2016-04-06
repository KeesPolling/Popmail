using Popmail.UILogic.Models;
using PopMail.DataAcces;
using System.Collections.Generic;

namespace Popmail.UILogic.ViewModels
{
    public static class EmailProviderList
    {
        public static List<EmailProviderPageViewModel> EmailProviders
        {
            get
            {
                var db = Database.DbConnection;
                var ProviderTable = db.Table<EmailProvider>().ToListAsync().Result;
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
