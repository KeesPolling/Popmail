
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace Popmail.UILogic.Models
{
    public class EmailProvider
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed, NotNull]
        public string Name { get; set; }
        
        public string AccountName{get;set;}
        
        public string ProviderUri { get; set; }
        
        public string ServiceName { get; set; }
        
        [NotNull]
        public string User { get; set; }
        
        public string Password { get; set; }

        [NotNull, ForeignKey(typeof(Folder))]     // Specify the foreign key
        public int InFolderId { get; set; }

        [NotNull, ForeignKey(typeof(Folder))]     // Specify the foreign key
        public int OutFolderId { get; set; }

        [NotNull, ForeignKey(typeof(Folder))]     // Specify the foreign key
        public int SentFolderId { get; set; }

        [NotNull, ForeignKey(typeof(Folder))]     // Specify the foreign key
        public int ConceptsFolderId { get; set; }

    }
}
