using System.Reflection.Metadata;
using SQLite.Net.Attributes;

namespace Popmail.UILogic.Models
{
    public class Settings
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed(name:"Name", order:1, Unique =true), NotNull]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
