using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PopMailDemo.MVVM.DataAcces;
using PopMailDemo.MVVM.Model;

namespace PopMailDemo.MVVM.ViewModel
{
    public static class EmailProviderListVM
    {
        public static List<EmailProviderVM> EmailProviders
        {
            get
            {
                var db = Database.DbConnection;
                var ProviderTable = db.Table<EmailProvider>().ToListAsync().Result;
                var emailProviders = new List<EmailProviderVM>();
                foreach (var Provider in ProviderTable)
                {
                    emailProviders.Add(new EmailProviderVM(Provider.Id));
                }
                return emailProviders;
            }
        }
    }
}
