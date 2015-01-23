using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PopMailDemo.MVVM.DataAcces;
using PopMailDemo.MVVM.Model;
using PopMailDemo.MVVM.Utilities;
using SQLite;

namespace PopMailDemo.MVVM.ViewModel
{
    public class EmailProviderVM : BindableBase
    {
        private EmailProvider emailProvider;
        private FolderVM inFolder;
        private FolderVM outFolder;
        private FolderVM sentFolder;
        private FolderVM conceptsFolder;

        public EmailProviderVM()
        {
            this.emailProvider = new EmailProvider();
        }
        public EmailProviderVM(EmailProvider emailProvider)
        {
            this.emailProvider = emailProvider;
        }

        public string Name
        {
            get
            {
                if (this.emailProvider == null)
                {
                    return string.Empty;
                }
                return this.emailProvider.Name;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    this.emailProvider.Name = value;
                    if (this.emailProvider.InFolderId == 0)
                    {
                        FolderVM rootFolder = new FolderVM(value, null);
                        inFolder = new FolderVM("In", rootFolder);
                        outFolder = new FolderVM("Out", rootFolder);
                        sentFolder = new FolderVM("Sent", rootFolder);
                        conceptsFolder = new FolderVM("Concepts", rootFolder);
                    }
                    this.OnPropertyChanged();
                }
            }
        }
        public string AccountName
        {
            get
            {
                if (this.emailProvider != null)
                {
                    return string.Empty;
                }
                return this.emailProvider.AccountName;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    this.emailProvider.AccountName = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string ProviderUri
        {
            get
            {
                if (this.emailProvider != null)
                {
                    return string.Empty;
                }
                return this.emailProvider.ProviderUri;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    this.emailProvider.ProviderUri = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string ServiceName
        {
            get
            {
                if (this.emailProvider != null)
                {
                    return string.Empty;
                }
                return this.emailProvider.ServiceName;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    this.emailProvider.ServiceName = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string Password
        {
            get
            {
                if (this.emailProvider != null)
                {
                    return string.Empty;
                }
                return this.emailProvider.Password;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    this.emailProvider.Password = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public FolderVM InFolder
        {
            get
            {
                if (this.emailProvider != null)
                {
                    return null;
                }
                return this.inFolder;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("InFolder", "In folder is mandatory");
                    }
                    this.inFolder = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public FolderVM OutFolder
        {
            get
            {
                if (this.emailProvider != null)
                {
                    return null;
                }
                return this.outFolder;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("OutFolder", "Out folder is mandatory");
                    }
                    this.outFolder = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public FolderVM SentFolder
        {
            get
            {
                if (this.emailProvider != null)
                {
                    return null;
                }
                return this.sentFolder;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("SentFolder", "Sent folder is mandatory");
                    }
                    this.sentFolder = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public FolderVM ConceptsFolder
        {
            get
            {
                if (this.emailProvider != null)
                {
                    return null;
                }
                return this.conceptsFolder;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("ConceptsFolder", "Concepts folder is mandatory");
                    }
                    this.conceptsFolder = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public async Task Save()
        {
            if (this.emailProvider != null)
            {
                if (this.emailProvider.Name != null)
                {
                    await inFolder.Save();
                    await outFolder.Save();
                    await sentFolder.Save();
                    await conceptsFolder.Save();
                    var db = Database.DbConnection;

                    if (this.emailProvider.Id == 0)
                    {
                        // New
                        var i = await db.InsertAsync(this.emailProvider);
                    }
                    else
                    {
                        // Update
                        var i = await db.UpdateAsync(this.emailProvider);
                    }
                }
            } 
         }
    }
}
