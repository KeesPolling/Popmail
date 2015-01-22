using SQLite;

namespace PopMailDemo.MVVM.Model
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
        public string User { get; set;}
        public string Password { get; set; }
        [NotNull]
        public int InFolder { get; set; }
        [NotNull]
        public int OutFolder { get; set; }
        [NotNull]
        public int SentFolder { get; set; }
        [NotNull]
        public int ConceptsFolder { get; set; }
    }
}
