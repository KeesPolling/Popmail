using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace Popmail.UILogic.Models
{
    public class Folders
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Name { get; set; }
        [Indexed, ForeignKey(typeof(Folders))] 
        public int Parent { get; set; }
    }
}
